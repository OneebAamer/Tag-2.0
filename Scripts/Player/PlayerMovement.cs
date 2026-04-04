using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public PlayerData playerData;
    public Camera playerCam;
    public GameObject playerBody;
    public Sliding slideManager;
    public Transform groundCheck;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    [Header("Movement")]
    public float gravity = -50f;
    public NetworkVariable<float> jumpHeight = new NetworkVariable<float>(4f);
    public float groundDistance = 0.4f;
    public LayerMask groundmask;

    [SerializeField] private float groundAcceleration = 18f;
    [SerializeField] private float groundDeceleration = 20f;
    [SerializeField] private float airAcceleration = 6f;
    [SerializeField] private float airDeceleration = 4f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float groundTurnAcceleration = 45f;

    [Header("Slope")]
    [SerializeField] private bool willSlideOnSlope = true;
    [SerializeField] private float slopeSpeed = 0.2f;

    private float coyoteCounter;
    private float serverJumpBufferCounter;

    private Vector3 horizontalVelocity;
    private Vector3 velocity;
    private Vector3 slopeVelocity;

    private Vector2 moveInput;
    private bool localJumpQueued;
    private bool serverJumpQueued;
    private bool sprintHeld;
    private bool crouchHeld;
    private bool ownerInputEnabled;

    [Header("Tag Push")]
    [SerializeField] public float tagPushDecay = 100f;

    private Vector3 tagPushVelocity;

    public Vector3 hitPointNormal;

    private InputAction MoveInputAction => moveAction != null ? moveAction.action : null;
    private InputAction JumpInputAction => jumpAction != null ? jumpAction.action : null;
    private InputAction SprintInputAction => sprintAction != null ? sprintAction.action : null;
    private InputAction CrouchInputAction => crouchAction != null ? crouchAction.action : null;

    public Vector2 RawMoveInputFromActions =>
        MoveInputAction != null
            ? Vector2.ClampMagnitude(MoveInputAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;

    public bool JumpPressedThisFrame =>
        JumpInputAction != null && JumpInputAction.WasPressedThisFrame();

    public bool SprintHeldFromAction =>
        SprintInputAction != null && SprintInputAction.IsPressed();

    public bool CrouchHeldFromAction =>
        CrouchInputAction != null && CrouchInputAction.IsPressed();

    public bool CrouchPressedThisFrame =>
        CrouchInputAction != null && CrouchInputAction.WasPressedThisFrame();

    public bool CrouchReleasedThisFrame =>
        CrouchInputAction != null && CrouchInputAction.WasReleasedThisFrame();

    public bool IsGroundedNow()
    {
        return Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundmask,
            QueryTriggerInteraction.Ignore
        );
    }

    public bool IsSlopeSliding
    {
        get
        {
            if (IsGroundedNow() &&
                Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) >= controller.slopeLimit;
            }

            return false;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCam != null)
                playerCam.gameObject.SetActive(false);
        }
        else
        {
            if (playerBody != null)
                playerBody.SetActive(false);

            EnableOwnerInput();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            DisableOwnerInput();
    }

    private void OnEnable()
    {
        if (IsSpawned && IsOwner)
            EnableOwnerInput();
    }

    private void OnDisable()
    {
        if (ownerInputEnabled)
            DisableOwnerInput();
    }

    private void EnableOwnerInput()
    {
        if (ownerInputEnabled)
            return;

        if (MoveInputAction != null)
            MoveInputAction.Enable();

        if (JumpInputAction != null)
        {
            JumpInputAction.Enable();
            JumpInputAction.performed += OnJumpPerformed;
        }

        if (SprintInputAction != null)
            SprintInputAction.Enable();

        if (CrouchInputAction != null)
            CrouchInputAction.Enable();

        ownerInputEnabled = true;
    }

    private void DisableOwnerInput()
    {
        if (!ownerInputEnabled)
            return;

        if (JumpInputAction != null)
            JumpInputAction.performed -= OnJumpPerformed;

        if (MoveInputAction != null)
            MoveInputAction.Disable();

        if (JumpInputAction != null)
            JumpInputAction.Disable();

        if (SprintInputAction != null)
            SprintInputAction.Disable();

        if (CrouchInputAction != null)
            CrouchInputAction.Disable();

        ownerInputEnabled = false;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner)
            return;

        if (playerData != null && playerData.isInMenu)
            return;

        localJumpQueued = true;
    }

    private void Update()
    {
        if (IsServer)
            SimulateMovement();

        if (!IsOwner)
            return;

        CaptureInput();
        SendInputServerRpc(moveInput, localJumpQueued, sprintHeld, crouchHeld);

        localJumpQueued = false;
    }

    private void CaptureInput()
    {
        if (playerData != null && playerData.isInMenu)
        {
            moveInput = Vector2.zero;
            sprintHeld = false;
            crouchHeld = false;
            localJumpQueued = false;
            return;
        }

        moveInput = MoveInputAction != null
            ? MoveInputAction.ReadValue<Vector2>()
            : Vector2.zero;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        sprintHeld = SprintInputAction != null && SprintInputAction.IsPressed();

        bool crouchPressed = CrouchInputAction != null && CrouchInputAction.IsPressed();
        bool slideForcedCrouch = slideManager != null && (slideManager.isCrouching || slideManager.isStuck);
        crouchHeld = crouchPressed || slideForcedCrouch;
    }

    [ServerRpc]
    private void SendInputServerRpc(Vector2 move, bool jumpPressed, bool sprint, bool crouch)
    {
        moveInput = move;
        sprintHeld = sprint;
        crouchHeld = crouch;

        if (jumpPressed)
        {
            serverJumpQueued = true;
            serverJumpBufferCounter = jumpBufferTime;
        }
    }

    private void SimulateMovement()
    {
        bool grounded = IsGroundedNow();

        if (playerData != null && playerData.isInMenu)
        {
            moveInput = Vector2.zero;
            sprintHeld = false;
            crouchHeld = false;
            serverJumpQueued = false;
            serverJumpBufferCounter = 0f;
        }

        if (serverJumpBufferCounter > 0f)
            serverJumpBufferCounter -= Time.deltaTime;
        else
            serverJumpQueued = false;

        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        if (grounded && velocity.y < 0f)
            velocity.y = -2f;

        float x = moveInput.x;
        float z = moveInput.y;

        Vector3 inputMove = (transform.right * x + transform.forward * z);
        inputMove = Vector3.ClampMagnitude(inputMove, 1f);

        bool slidingNow = slideManager != null && slideManager.sliding;

        if (!slidingNow)
        {
            float speed = playerData.movementSpeed;

            if (crouchHeld && grounded)
                speed *= 0.5f;
            else if (sprintHeld)
                speed *= 1.5f;

            Vector3 targetHorizontalVelocity = inputMove * speed;

            float accel;

            if (inputMove.sqrMagnitude <= 0.01f)
            {
                accel = grounded ? groundDeceleration : airDeceleration;
            }
            else
            {
                bool reversing =
                    grounded &&
                    horizontalVelocity.sqrMagnitude > 0.01f &&
                    targetHorizontalVelocity.sqrMagnitude > 0.01f &&
                    Vector3.Dot(horizontalVelocity.normalized, targetHorizontalVelocity.normalized) < 0f;

                if (reversing)
                    accel = groundTurnAcceleration;
                else
                    accel = grounded ? groundAcceleration : airAcceleration;
            }

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetHorizontalVelocity,
                accel * Time.deltaTime
            );

            if (grounded && inputMove.sqrMagnitude <= 0.01f && horizontalVelocity.sqrMagnitude < 0.01f)
                horizontalVelocity = Vector3.zero;
        }

        if (serverJumpQueued &&
            serverJumpBufferCounter > 0f &&
            coyoteCounter > 0f &&
            !IsSlopeSliding)
        {
            velocity.y = Mathf.Sqrt(jumpHeight.Value * -2f * gravity);
            coyoteCounter = 0f;
            serverJumpQueued = false;
            serverJumpBufferCounter = 0f;
        }

        velocity.y += gravity * Time.deltaTime;

        if (willSlideOnSlope && IsSlopeSliding)
        {
            slopeVelocity = new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        else
        {
            slopeVelocity = Vector3.zero;
        }

        Vector3 totalMove = horizontalVelocity + tagPushVelocity + slopeVelocity + Vector3.up * velocity.y;
        controller.Move(totalMove * Time.deltaTime);

        tagPushVelocity = Vector3.MoveTowards(
            tagPushVelocity,
            Vector3.zero,
            tagPushDecay * Time.deltaTime
        );
    }

    public void ApplyJumpBoost(float amount, float duration)
    {
        if (!IsServer) return;

        StartCoroutine(JumpBoostCoroutine(amount, duration));
    }

    private IEnumerator JumpBoostCoroutine(float amount, float duration)
    {
        jumpHeight.Value += amount;
        yield return new WaitForSeconds(duration);
        jumpHeight.Value -= amount;
    }

    public void SetHorizontalMomentum(Vector3 worldVelocity)
    {
        horizontalVelocity = new Vector3(worldVelocity.x, 0f, worldVelocity.z);
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        Vector3 projected = Vector3.ProjectOnPlane(direction, hitPointNormal);

        if (projected.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return projected.normalized;
    }

    public void ApplyTagPush(Vector3 worldVelocity)
    {
        if (!IsServer)
            return;

        worldVelocity.y = 0f; // keep it horizontal for now
        tagPushVelocity = worldVelocity;
    }

    public void ClearTagPush()
    {
        tagPushVelocity = Vector3.zero;
    }
}