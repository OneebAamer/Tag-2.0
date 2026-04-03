using UnityEngine;
using Unity.Netcode;

public abstract class Interactable : NetworkBehaviour
{
    public abstract void onInteract(GameObject player);
}
