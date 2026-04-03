using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using System.Linq;

public class GameUI : NetworkBehaviour
{
    public TMP_Text middleText;
    private GameObject joinCodeText;
    private TMP_Text lobbyInfoText;

    public PlayersManager playersManager;
    public GameManager gameManager;

    private void Update()
    {
        updateLobbyInfo();
    }

    private void updateLobbyInfo()
    {
        if (!IsOwner) { return; }
        // IN GAME
        if (gameManager.InGame.Value)
        {
            inGameUI();
        }
        // IN LOBBY
        else { lobbyUI(); }
    }

    private void inGameUI()
    {
        joinCodeText.SetActive(false);
        lobbyInfoText.text = "";
        middleText.text = "";
    }

    private void lobbyUI()
    {
        joinCodeText.SetActive(true);
        if (gameManager.TaggedPlayerIds.Count > 0)
        {
            middleText.text = FetchMatchOutcome() ? "DEFEAT" : "VICTORY";
        }

        if (playersManager.getPlayerCount() < 2)
        {
            lobbyInfoText.text = "Waiting for 1 More Player";
            return;
        }
        lobbyInfoText.text = IsHost
            ? "Press Enter to Start"
            : "Waiting for the Host to Start";
    }

    private bool FetchMatchOutcome()
    {
        return false;
        //return gameManager.TaggedPlayerIds.Contains(playerData.id.Value);
    }
}