using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class HitPlayer : NetworkBehaviour
{
    public PlayerData playerData;
    private GameManager gameManager;
    public Camera playerCam;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if(Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit rayHit, PlayerData.hitDistance))
            {
                GameObject objectHit = rayHit.transform.gameObject;
                manageHit(objectHit);
            }
        }
    }

    void manageHit(GameObject objectHit) {
        if (objectHit.tag.Equals("Player"))
        {
            hitPlayer(objectHit);
            return;
        }
        Interactable interactable = objectHit.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.onInteract(this.gameObject);
            return;
        }
    }

    void hitPlayer(GameObject playerHit){
        // Client side verification
        PlayerData pd2 = playerHit.GetComponent<PlayerData>();
        if (!playerData.IsTagged.Value || pd2.IsTagged.Value || pd2.IsSpectating.Value) { return; }
        if (Vector3.Distance(this.gameObject.transform.position, playerHit.transform.position) > PlayerData.hitDistance) { return; }

        gameManager.TransferPlayerTaggedServerRpc(playerData.OwnerClientId, pd2.OwnerClientId);
    }
}
