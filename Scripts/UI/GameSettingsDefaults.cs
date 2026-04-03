using UnityEngine;
using TMPro;

public class GameSettingsDefaults : MonoBehaviour
{
    private GameManager gameManager;
    public TMP_Dropdown mapSelect;
    public TMP_Dropdown gamemodeSelect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        mapSelect.value = gameManager.mapIndex;
        gamemodeSelect.value = gameManager.gamemodeIndex;
    }
}
