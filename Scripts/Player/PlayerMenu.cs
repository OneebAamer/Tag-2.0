using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class PlayerMenu : NetworkBehaviour
{
    public PlayerData playerData;
    public MenuBehaviour playerSettingsMenu;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer) { return; }
        if(Input.GetKeyDown(KeyCode.Escape) && !playerData.isInMenu){
            playerSettingsMenu.openMenu();
        }
    }

    public void LeaveGame(){
        if(IsHost || IsServer){
            Debug.Log("server/host attempt to leave");
            LeaveGameServerRpc();
        }
        else if(IsClient){
            Debug.Log("Client attempt to leave");
            NetworkManager.Singleton.Shutdown();
        }
        else{
            Debug.Log("Error");
        }
    }

    [ServerRpc]
    public void LeaveGameServerRpc(){
        NetworkManager.Singleton.Shutdown(discardMessageQueue: true);
    }

    public void updateRoundDuration(float gameDuration)
    {
        gameManager.roundTime = (int)gameDuration;
    }

    public void updateGamemode(int gamemodeIndex)
    {
        gameManager.gamemodeIndex = gamemodeIndex;
    }

    public void updateMap(int mapIndex)
    {
        gameManager.mapIndex = mapIndex;
    }
}
