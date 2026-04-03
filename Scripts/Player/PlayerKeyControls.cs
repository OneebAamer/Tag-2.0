using UnityEngine;
using Unity.Netcode;

public class PlayerStartGame : NetworkBehaviour
{
    private GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.Return))
        {
            gameManager.initializeGameServerRpc();
        }
    }
}
