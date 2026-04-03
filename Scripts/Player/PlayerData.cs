using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerData : NetworkBehaviour
{
    [Header("Network State")]
    public NetworkVariable<FixedString64Bytes> Username = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsTagged = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsSpectating = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private PlayersManager playersManager;
    private UIManager uiManager;
    private GameManager gameManager;

    [Header("Camera")]
    [SerializeField] private Camera ownerCamera;
    //[SerializeField] private AudioListener ownerAudioListener;
    [SerializeField] private Transform spectateAnchor;

    [Header("Movement")]
    [SerializeField] public float movementSpeed = 8f;
    [SerializeField] public float mouseSensitivity = 400f;
    private const int maxSens = 1600;

    [Header("Visuals/UI")]
    [SerializeField] public PlayerVisuals playerVisuals;
    [SerializeField] public PlayerUI playerUI;

    [Header("Metadata")]
    public bool isInMenu = false;
    public const float hitDistance = 5f;

    [Header("Spectator")]
    [SerializeField] private MonoBehaviour[] disableWhenSpectating;
    [SerializeField] private Collider[] disableCollidersWhenSpectating;
    [SerializeField] private GameObject[] hideWhenSpectating;

    [Header("Explosion")]
    [SerializeField] private VisualEffect explosionVfx;

    private SpectatorController spectatorController;
    public Transform SpectateAnchor => spectateAnchor != null ? spectateAnchor : transform;

    public override void OnNetworkSpawn()
    {
        GetExternalComponents(); 

        if (IsOwner)
        {
            spectatorController = FindFirstObjectByType<SpectatorController>(FindObjectsInactive.Include);
        }

        // Add hooks
        Username.OnValueChanged += OnUsernameChanged;
        IsTagged.OnValueChanged += OnTaggedChanged;
        IsSpectating.OnValueChanged += OnSpectatingChanged;

        // Add user to player manager
        playersManager.addPlayer(OwnerClientId, this);

        // Apply user attributes
        ApplyUsername(Username.Value.ToString());
        playerVisuals.ApplyTagVisuals(IsTagged.Value);
        ApplyInitialSettings();
        ApplySpectatorState(gameManager.InGame.Value);
    }

    private void GetExternalComponents()
    {
        playersManager = GameObject.Find("PlayersManager").GetComponent<PlayersManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }

    public override void OnNetworkDespawn()
    {
        ApplySpectatorState(false);

        Username.OnValueChanged -= OnUsernameChanged;
        IsTagged.OnValueChanged -= OnTaggedChanged;
        IsSpectating.OnValueChanged -= OnSpectatingChanged;

        if (IsOwner)
        {
            gameManager.resetGameState();
            uiManager.showMainMenu();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        playersManager.removePlayer(OwnerClientId);
    }

    public string getUsername()
    {
        return Username.Value.ToString();
    }

    private void OnUsernameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        ApplyUsername(newValue.ToString());
    }

    private void OnTaggedChanged(bool previousValue, bool newValue)
    {
        playerVisuals.ApplyTagVisuals(newValue);
    }

    private void OnSpectatingChanged(bool previousValue, bool newValue)
    {
        ApplySpectatorState(newValue);
    }

    private void ApplyUsername(string username)
    {
        gameObject.name = string.IsNullOrWhiteSpace(username) ? "Player" : username;
    }

    public void changeSensitivity(float newSens)
    {
        mouseSensitivity = newSens * maxSens;
    }

    public void changeFOV(float newFOV)
    {
        ownerCamera.fieldOfView = (int)newFOV;
    }

    private void ApplyInitialSettings()
    {
        if (PlayerPrefs.GetFloat("FOV") != 0.0f)
            changeFOV(PlayerPrefs.GetFloat("FOV"));
        if (PlayerPrefs.GetFloat("Sensitivity") != 0.0f)
            changeSensitivity(PlayerPrefs.GetFloat("Sensitivity"));
    }

    private void ApplySpectatorState(bool isSpectating)
    {
        IsSpectating.Value = isSpectating;
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = !isSpectating;
        }

        if (!IsOwner) playerVisuals.ApplySpectatorVisuals(isSpectating);

        if (ownerCamera != null)
            ownerCamera.enabled = !isSpectating;

        if (spectatorController != null)
            spectatorController.SetSpectating(this, isSpectating);
    }

    [ClientRpc]
    public void PlayExplosionClientRpc()
    {
        if (explosionVfx == null) return;

        explosionVfx.Reinit();
        explosionVfx.Play();
    }
}