using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Tag.Core.Singleton;
using TMPro;
using System.Linq;

public class PlayersManager : Singleton<PlayersManager>
{
    public Dictionary<ulong, PlayerData> idToPlayer = new Dictionary<ulong, PlayerData>();

    private void Start(){
        NetworkObject.DontDestroyWithOwner = true;
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if(IsServer){
                Debug.Log($"{id} just connected...");
            }
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if(IsServer){
                Debug.Log($"{id} just disconnected...");
            }
            removePlayer(id);
            Debug.Log("Client disconnected");
        };
    }

    void Update(){
        if (NetworkManager.Singleton.ShutdownInProgress){
            Debug.Log("Server shutdown");
        }
    }

    public void addPlayer(ulong id, PlayerData playerData)
    {
        idToPlayer[id] = playerData;
    }

    public PlayerData getPlayer(ulong id)
    {
        return idToPlayer[id];
    }

    public PlayerData[] GetAllPlayers()
    {
        return idToPlayer.Values.ToArray();
    }

    public PlayerData[] GetAllUntaggedPlayers()
    {
        return this
        .GetAllPlayers()
        .Where(player => player != null && !player.IsTagged.Value).ToArray();
    }

    public PlayerData[] GetAllAlivePlayers()
    {
        return this
        .GetAllPlayers()
        .Where(player => player != null && !player.IsSpectating.Value).ToArray();
    }

    public GameObject[] GetAllPlayerObjects()
    {
        return idToPlayer.Values
            .Select(playerData => playerData.gameObject)
            .ToArray();
    }

    public void removePlayer(ulong id)
    {
        idToPlayer.Remove(id);
    }

    public int getPlayerCount()
    {
        return NetworkManager.Singleton.ConnectedClients.Count;
    }
}
