using UnityEngine;
using Unity.Netcode;

public class GameSettings : Interactable
{
    public override void onInteract(GameObject player)
    {
        PlayerData playerData = player.GetComponent<PlayerData>();
        if (!IsHost || playerData.isInMenu) { return; }

        MenuBehaviour gameSettings = player.transform.Find("gameSettings").gameObject.GetComponent<MenuBehaviour>();
        gameSettings.openMenu();
    }
}
