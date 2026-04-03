using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerUI : NetworkBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject nametag;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TMP_Text crosshair;
    [SerializeField] private TMP_Text lobbyInfo;
    [SerializeField] private TMP_Text middleText;
    [SerializeField] private TMP_Text upperRightText;

    private GameObject lobbyUI;
    private PlayersManager playersManager;
    private GameManager gameManager;
    private Transform mainCamTransform;

    public override void OnNetworkSpawn()
    {
        playerData.Username.OnValueChanged += OnNameChanged;
        playerData.IsTagged.OnValueChanged += OnTaggedChanged;

        lobbyUI = GameObject.Find("LobbyUI");

        UpdateNameVisual(playerData.getUsername());

        if (IsOwner)
        {
            SubmitNameServerRpc(PlayerPrefs.GetString("Name"));
            nametag.SetActive(false);
            UpdateCrosshair(playerData.IsTagged.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        playerData.Username.OnValueChanged -= OnNameChanged;
        playerData.IsTagged.OnValueChanged -= OnTaggedChanged;
        EnableGameUI();
        EnableLobbyUI();
    }

    private void Start()
    {
        playersManager = GameObject.Find("PlayersManager").GetComponent<PlayersManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }

        UpdateLobbyInfo();
        NametagFollowCamera();
    }

    private void OnTaggedChanged(bool oldValue, bool newValue)
    {
        if (!IsOwner) return;

        UpdateCrosshair(newValue);
    }

    public void UpdateCrosshair(bool isTagged)
    {
        if (crosshair == null)
        {
            Debug.LogWarning($"Crosshair ref missing on {gameObject.name}");
            return;
        }

        crosshair.color = isTagged ? Color.red : Color.white;
    }

    private void UpdateLobbyInfo()
    {
        if (!IsOwner) return;

        ClearUI();

        if (playerData.IsSpectating.Value) return;

        if (gameManager.InGame.Value)
        {
            EnableGameUI();
        }
        else
        {
            EnableLobbyUI();
        }
    }

    private void EnableGameUI()
    {
        if (gameManager.InPreround.Value)
        {
            middleText.text = gameManager.PreroundTimer.Value.ToString();
        }
        else if (gameManager.InRound.Value)
        {
            upperRightText.text = gameManager.RoundTimer.Value.ToString();
        }
    }

    private void EnableLobbyUI()
    {
        lobbyUI.SetActive(true);

        if (gameManager.TaggedPlayerIds.Count > 0)
        {
            middleText.text = IsMatchLoser() ? "DEFEAT" : "VICTORY";
        }
        else
        {
            middleText.text = "";
        }

        if (playersManager.getPlayerCount() < 2)
        {
            lobbyInfo.text = "Waiting for 1 More Player";
            return;
        }

        lobbyInfo.text = IsHost ? "Press Enter to Start" : "Waiting for the Host to Start";
    }

    private void ClearUI()
    {
        lobbyUI.SetActive(false);
        lobbyInfo.text = "";
        middleText.text = "";
        upperRightText.text = "";
    }

    private void NametagFollowCamera()
    {
        if (nametag == null || mainCamTransform == null) return;

        nametag.transform.LookAt(
            nametag.transform.position + mainCamTransform.rotation * Vector3.forward,
            mainCamTransform.rotation * Vector3.up
        );
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string name)
    {
        playerData.Username.Value = name;
    }

    private void OnNameChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        UpdateNameVisual(newValue.ToString());
    }

    private void UpdateNameVisual(string name)
    {
        nametag.GetComponentInChildren<TextMeshProUGUI>().text = name;
    }

    private bool IsMatchLoser()
    {
        return gameManager.TaggedPlayerIds.Contains(playerData.OwnerClientId);
    }
}