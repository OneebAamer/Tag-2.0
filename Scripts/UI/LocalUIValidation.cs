using UnityEngine;
using Unity.Netcode;

public class LocalUIValidation : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.gameObject.SetActive(false);
        }
    }
}
