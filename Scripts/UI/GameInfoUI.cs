using UnityEngine;
using TMPro;
using System.Linq;

public class GameInfoUI : MonoBehaviour
{
    public TMP_Text playerList;
    public TMP_Text gameSettingsList;
    public PlayersManager playersManager;
    public GameManager gameManager;
    void Update()
    {
        playerList.text = "";
        gameSettingsList.text = "";
        foreach (string username in playersManager
            .GetAllPlayers()
            .Where(player => player != null)
            .Select(player => player.Username.Value.ToString())) 
        {
            playerList.text += username;
            playerList.text += "<br>";
        }

        gameSettingsList.text += "Round Time: " + gameManager.roundTime + "s<br>";
        gameSettingsList.text += "Gamemode: " + gameManager.GetGamemode();
    }
}
