using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using System.Collections.Generic;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayersManager playersManager;
    [SerializeField] private GameObject[] gameSpawns;
    private string[] gamemodes = { "Tag", "Infected", "Hot Potato" };
    [SerializeField] private GameObject lobbySpawn;
    [SerializeField] private GameTimeManager gameTimeManager;

    [Header("Settings")]
    // TODO Change to 2 when Prod ready
    public const int minPlayerCount = 1;
    private const int maxPreroundDuration = 10;
    private const int maxPostroundDuration = 3;
    [SerializeField] public int roundTime = 60;
    public NetworkVariable<int> RoundTimer = new NetworkVariable<int>();
    public NetworkVariable<int> PreroundTimer = new NetworkVariable<int>(maxPreroundDuration);
    public NetworkVariable<int> PostroundTimer = new NetworkVariable<int>(maxPostroundDuration);
    [Header("Game State")]
    public NetworkVariable<bool> InRound = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> InPreround = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> InPostround = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> InGame = new NetworkVariable<bool>(false);
    [SerializeField] public int gamemodeIndex = 0;
    [SerializeField] public int mapIndex = 0;

    [Header("Tag Push")]
    [SerializeField] private float taggedPlayerPushStrength = 5f;

    public NetworkList<ulong> TaggedPlayerIds = new NetworkList<ulong>();

    private Coroutine roundTimerCoroutine;
    private Coroutine preroundTimerCoroutine;
    private Coroutine postroundTimerCoroutine;

    private void Start()
    {
        NetworkObject.DontDestroyWithOwner = true;
        roundTime = (int)PlayerPrefs.GetFloat("GameTimer");
    }

    private void Update()
    {
        if (!IsServer)
            return;
        if (TaggedPlayerIds.Count == playersManager.getPlayerCount())
        {
            stopCoroutines();
            EndGame();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TransferPlayerTaggedServerRpc(ulong fromClientId, ulong toClientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(fromClientId, out var fromClient) ||
            fromClient.PlayerObject == null)
        {
            Debug.LogWarning($"Could not find from player for clientId {fromClientId}");
            return;
        }

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(toClientId, out var toClient) ||
            toClient.PlayerObject == null)
        {
            Debug.LogWarning($"Could not find to player for clientId {toClientId}");
            return;
        }

        PlayerData fromPlayer = fromClient.PlayerObject.GetComponent<PlayerData>();
        PlayerData toPlayer = toClient.PlayerObject.GetComponent<PlayerData>();

        // Server side verification
        if (!fromPlayer.IsTagged.Value || toPlayer.IsTagged.Value || toPlayer.IsSpectating.Value) { return; }
        if (Vector3.Distance(fromPlayer.gameObject.transform.position, toPlayer.gameObject.transform.position) > PlayerData.hitDistance) { return; }

        if (!GetGamemode().Equals("Infected"))
        {
            TaggedPlayerIds.Remove(fromPlayer.OwnerClientId);
            fromPlayer.IsTagged.Value = false;
        }
        TaggedPlayerIds.Add(toPlayer.OwnerClientId);
        toPlayer.IsTagged.Value = true;
        PlayerMovement toPlayerMovement = toPlayer.gameObject.GetComponent<PlayerMovement>();

        // Compute push direction from tagger -> tagged
        Vector3 pushDir = toPlayer.transform.position - fromPlayer.transform.position;
        pushDir.y = 0f;

        if (pushDir.sqrMagnitude < 0.0001f)
            pushDir = fromClient.PlayerObject.transform.forward;
        toPlayerMovement.ApplyTagPush(pushDir * taggedPlayerPushStrength);
    }

    [ServerRpc]
    public void initializeGameServerRpc()
    {
        if (playersManager.getPlayerCount() < minPlayerCount) { return; }
        if (InGame.Value)
            return;

        InGame.Value = true;
        TaggedPlayerIds.Clear();
        if (GetGamemode().Equals("Hot Potato"))
        {
            StartHotPotatoGame();
        }
        else
        {
            StartPreround();
        }
    }


    public void StartHotPotatoGame()
    {
        StartHotPotatoPreround();
    }

    private void StartPreround()
    {
        InPreround.Value = true;
        TeleportAllAlivePlayers(gameSpawns[mapIndex].transform.position);

        preroundTimerCoroutine = StartCoroutine(gameTimeManager.CountdownRoutine(maxPreroundDuration, PreroundTimer, () =>
        {
            InPreround.Value = false;
            StartRound();
        }));
    }

    private void StartHotPotatoPreround()
    {
        InPreround.Value = true;
        TeleportAllAlivePlayers(gameSpawns[mapIndex].transform.position);

        preroundTimerCoroutine = StartCoroutine(gameTimeManager.CountdownRoutine(maxPreroundDuration, PreroundTimer, () =>
        {
            InPreround.Value = false;
            StartHotPotatoRound();
        }));
    }

    private void StartHotPotatoRound()
    {
        InRound.Value = true;

        setHalfTaggedPlayers();

        roundTimerCoroutine = StartCoroutine(gameTimeManager.CountdownRoutine(roundTime, RoundTimer, () =>
        {
            InRound.Value = false;
            EndHotPotatoRound();
        }
        ));
    }

    private void StartRound()
    {
        InRound.Value = true;

        setInitialTaggedPlayer();

        roundTimerCoroutine = StartCoroutine(gameTimeManager.CountdownRoutine(roundTime, RoundTimer, () =>
        {
            InRound.Value = false;
            EndRound();
        }
        ));
    }

    private void setHalfTaggedPlayers()
    {
        int halfAlivePlayerCount = playersManager.GetAllAlivePlayers().Length / 2;
        for (int i = 0; i < halfAlivePlayerCount; i++)
        {
            SetRandomUntaggedAlivePlayer();
        }
    }

    private void setInitialTaggedPlayer()
    {
        SetRandomUntaggedAlivePlayer();
    }

    private void EndHotPotatoRound()
    {
        // Explode all players
        InPostround.Value = true;
        EliminateTaggedPlayers();
        postroundTimerCoroutine = StartCoroutine(gameTimeManager.CountdownRoutine(maxPostroundDuration, PostroundTimer, () =>
        {
            InPostround.Value = false;
            // Check if only 1 person remaining, end game
            if (playersManager.GetAllAlivePlayers().Length == 1)
            {
                EndGame();
            }
            else
            {
                StartHotPotatoPreround();
            }
        }
        ));
    }

    private void EndRound()
    {
        // Explode all players
        EliminateTaggedPlayers();
        InPostround.Value = true;
        postroundTimerCoroutine = StartCoroutine(gameTimeManager.CountdownRoutine(maxPostroundDuration, PostroundTimer, () =>
        {
            InPostround.Value = false;
            EndGame();
        }
        ));
    }

    private void EndGame()
    {
        resetGameState(); 

        InGame.Value = false;

        calculateWinners();

        ResetAllPlayerStatusServerRpc();

        TeleportAllPlayers(lobbySpawn.transform.position);
    }

    private void calculateWinners()
    {
        TaggedPlayerIds.Clear();

        foreach (ulong id in playersManager
            .GetAllPlayers()
            .Where(player => player != null && player.IsTagged.Value)
            .Select(player => player.OwnerClientId))
        {
            TaggedPlayerIds.Add(id);
        }

        if (TaggedPlayerIds.Count < 1) { return; }
        // If everyone lost, no losers so clear losers
        if (TaggedPlayerIds.Count == playersManager.getPlayerCount())
        {
            TaggedPlayerIds.Clear();
        }
    }

    private void TeleportAllAlivePlayers(Vector3 position)
    {
        GameObject[] alivePlayers = playersManager.GetAllAlivePlayers()
            .Select(player => player.gameObject).ToArray();
        StartCoroutine(TeleportPlayersRoutine(alivePlayers, position));
    }

    private void TeleportAllPlayers(Vector3 position)
    {
        StartCoroutine(TeleportPlayersRoutine(playersManager.GetAllPlayerObjects(), position));
    }

    private IEnumerator TeleportPlayersRoutine(GameObject[] playerObjects, Vector3 position)
    {
        foreach (GameObject playerObject in playerObjects)
        {
            if (playerObject == null)
                continue;

            PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            playerObject.transform.position = position;
        }

        yield return new WaitForSeconds(1f);

        foreach (GameObject playerObject in playerObjects)
        {
            if (playerObject == null)
                continue;

            PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }

    private void SetRandomUntaggedAlivePlayer()
    {
        PlayerData[] allPlayers = playersManager
            .GetAllUntaggedPlayers()
            .Where(player => player != null && !player.IsSpectating.Value).ToArray();

        int randomIndex = Random.Range(0, allPlayers.Length);
        allPlayers[randomIndex].IsTagged.Value = true;
        TaggedPlayerIds.Add(allPlayers[randomIndex].OwnerClientId);
    }

    private void EliminateTaggedPlayers()
    {
        foreach (PlayerData player in playersManager.GetAllPlayers())
        {
            if (player.IsSpectating.Value) continue;

            if (player.IsTagged.Value)
            {
                // 
                player.IsTagged.Value = false;
                player.IsSpectating.Value = true;
                player.PlayExplosionClientRpc();
            }
        }
    }

    [ServerRpc]
    private void ResetAllPlayerStatusServerRpc()
    {
        foreach (PlayerData player in playersManager.GetAllPlayers())
        {
            player.IsTagged.Value = false;
            player.IsSpectating.Value = false;
        }
    }

    public void resetGameState()
    {
        InGame.Value = false;
        InRound.Value = false;
        InPreround.Value = false;
        //mapIndex = 0;
        //gamemode = "Tag";
        stopCoroutines();
    }
    private void stopCoroutines()
    {
        if (roundTimerCoroutine != null)
            StopCoroutine(roundTimerCoroutine);
        if (preroundTimerCoroutine != null)
            StopCoroutine(preroundTimerCoroutine);
    }

    public string GetGamemode()
    {
        return gamemodes[gamemodeIndex];
    }
}