using UnityEngine;
using Unity.Netcode;

public class Sliding : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform cameraPosition;
    private PlayerMovement playerMovement;
    private CharacterController controller;

    [Header("Slide")]
    public float maxSlideTime = 0.55f;
    public float slideForce = 10f;

    [Header("Crouch")]
    public float controllerCrouchHeight = 1f;
    public float controllerStandHeight = 2f;
    public Vector3 crouchCentre = new Vector3(0f, 0.25f, 0f);
    public Vector3 standCentre = new Vector3(0f, 0f, 0f);

    [Header("Camera Visuals")]
    public Vector3 standingPos;
    public Vector3 crouchingPos;
    public float cameraLerpSpeed = 12f;

    private float slideTimer;
    private float horizontalInput;
    private float verticalInput;

    private bool crouchHeldOnServer;
    private bool bufferedSlide;

    private NetworkVariable<bool> netIsCrouching = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> netSliding = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> netIsStuck = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool isCrouching => netIsCrouching.Value;
    public bool isStuck => netIsStuck.Value;
    public bool sliding => netSliding.Value;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();

        if (cameraPosition == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraPosition = cam.transform;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            ReadLocalInput();
        }

        Vector3 target = (netIsCrouching.Value || netSliding.Value) ? crouchingPos : standingPos;

        if (IsOwner && cameraPosition != null)
        {
            cameraPosition.localPosition = Vector3.Lerp(
                cameraPosition.localPosition,
                target,
                cameraLerpSpeed * Time.deltaTime
            );
        }

        if (!IsServer || controller == null)
            return;

        ApplyServerControllerHeight();

        if (bufferedSlide && playerMovement.IsGroundedNow() && !netSliding.Value)
        {
            StartSlide();
        }

        if (netSliding.Value)
        {
            DoServerSlideMovement();
        }
    }

    private void ReadLocalInput()
    {
        if (playerMovement == null || controller == null)
            return;

        Vector2 moveInput = playerMovement.RawMoveInputFromActions;
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

        bool moveHeld = moveInput.sqrMagnitude > 0.0001f;
        bool crouchHeld = playerMovement.CrouchHeldFromAction;
        bool crouchPressed = playerMovement.CrouchPressedThisFrame;
        bool crouchReleased = playerMovement.CrouchReleasedThisFrame;
        bool sprintHeld = playerMovement.SprintHeldFromAction;
        bool jumpPressed = playerMovement.JumpPressedThisFrame;
        bool grounded = playerMovement.IsGroundedNow();

        bool queueSlideOnLand = false;

        if (crouchPressed && !grounded && sprintHeld && moveHeld)
        {
            queueSlideOnLand = true;
        }

        SendSlideInputServerRpc(
            horizontalInput,
            verticalInput,
            crouchHeld,
            crouchPressed,
            crouchReleased,
            sprintHeld,
            moveHeld,
            jumpPressed,
            grounded,
            queueSlideOnLand
        );
    }

    [ServerRpc]
    private void SendSlideInputServerRpc(
        float horizontal,
        float vertical,
        bool crouchHeld,
        bool crouchPressed,
        bool crouchReleased,
        bool sprintHeld,
        bool moveHeld,
        bool jumpPressed,
        bool grounded,
        bool queueSlideOnLand)
    {
        horizontalInput = horizontal;
        verticalInput = vertical;
        crouchHeldOnServer = crouchHeld;

        if (netIsStuck.Value)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 0.5f))
            {
                netIsStuck.Value = false;
                netIsCrouching.Value = false;
            }
            else
            {
                netIsCrouching.Value = true;
            }
        }

        if (queueSlideOnLand)
        {
            bufferedSlide = true;
            return;
        }

        if (crouchPressed && sprintHeld && grounded && moveHeld)
        {
            StartSlide();
            return;
        }

        if (crouchPressed && !netSliding.Value && !bufferedSlide)
        {
            netIsCrouching.Value = true;
        }

        if (jumpPressed && netSliding.Value)
        {
            netSliding.Value = false;
            bufferedSlide = false;
            return;
        }

        if (crouchReleased)
        {
            bufferedSlide = false;

            if (netSliding.Value)
            {
                netSliding.Value = false;
            }

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 0.5f))
            {
                netIsStuck.Value = true;
                netIsCrouching.Value = true;
            }
            else
            {
                netIsStuck.Value = false;
                netIsCrouching.Value = false;
            }
        }

        if (crouchHeld && !crouchReleased && !netSliding.Value && !bufferedSlide)
        {
            netIsCrouching.Value = true;
        }
    }

    private void StartSlide()
    {
        bufferedSlide = false;
        netSliding.Value = true;
        netIsCrouching.Value = true;
        slideTimer = maxSlideTime;
    }

    private void CancelSlide()
    {
        netSliding.Value = false;
        bufferedSlide = false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 0.5f))
        {
            netIsStuck.Value = true;
            netIsCrouching.Value = true;
        }
        else
        {
            netIsStuck.Value = false;
            netIsCrouching.Value = false;
        }
    }

    private void ApplyServerControllerHeight()
    {
        float targetHeight = (netIsCrouching.Value || netSliding.Value) ? controllerCrouchHeight : controllerStandHeight;
        Vector3 targetCenter = (netIsCrouching.Value || netSliding.Value) ? crouchCentre : standCentre;

        controller.height = Mathf.Lerp(controller.height, targetHeight, cameraLerpSpeed * Time.deltaTime);
        controller.center = Vector3.Lerp(controller.center, targetCenter, cameraLerpSpeed * Time.deltaTime);
    }

    private void DoServerSlideMovement()
    {
        if (playerMovement == null || controller == null)
            return;

        Transform moveBasis = orientation != null ? orientation : transform;

        Vector3 inputDirection = moveBasis.forward * verticalInput + moveBasis.right * horizontalInput;
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        if (!playerMovement.IsSlopeSliding)
        {
            controller.Move(inputDirection * slideForce * Time.deltaTime);
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
            {
                netSliding.Value = false;

                if (!crouchHeldOnServer)
                {
                    if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 0.5f))
                    {
                        netIsStuck.Value = true;
                        netIsCrouching.Value = true;
                    }
                    else
                    {
                        netIsStuck.Value = false;
                        netIsCrouching.Value = false;
                    }
                }
            }
        }
        else
        {
            Vector3 slopeMove = playerMovement.GetSlopeMoveDirection(inputDirection);
            controller.Move(slopeMove * slideForce * Time.deltaTime);
        }
    }
}