using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
/// <summary>
/// Lets players set their name and synchronizes it to others.
/// </summary>
public class LobbyFieldsVerification : NetworkBehaviour
{
    public TMP_InputField enterUsername;
    public TMP_InputField joinCode;
    void Start(){
        enterUsername.text = PlayerPrefs.GetString("Name");
    }

    public bool isJoinCodeValid()
    {
        return joinCode.text != "" && joinCode.text.Length == 6;
    }
}