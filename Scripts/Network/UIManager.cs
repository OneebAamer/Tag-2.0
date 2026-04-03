using Tag.Core.Singleton;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class UIManager : Singleton<UIManager>
{

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TMP_InputField joinCodeInput;

    public GameObject joinCodeText;
    [SerializeField] private GameObject mainMenu;
    public TMP_InputField enterUsername;

    private bool hasServerStarted;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        // START HOST
        startHostButton?.onClick.AddListener(async () =>
        {
            // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
            // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
            // traffic through the relay, else it just uses a LAN type (UNET) communication.
            if (RelayManager.Instance.IsRelayEnabled){
                await RelayManager.Instance.SetupRelay();
                setUsername();
            }

            if (NetworkManager.Singleton.StartHost()){
                Debug.Log("Host started...");
                displayJoinCode();
                hideMainMenu();
            }
            else
                Debug.Log("Unable to start host...");
        });

        // START CLIENT
        startClientButton?.onClick.AddListener(async () =>
        {
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text)){
                await RelayManager.Instance.JoinRelay(joinCodeInput.text);
                setUsername();
            }
            if(NetworkManager.Singleton.StartClient()){
                Debug.Log("Client started...");
                displayJoinCode();
                hideMainMenu();
            }
            else
                Debug.Log("Unable to start client...");
        });
        // STATUS TYPE CALLBACKS
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };
    }

    private void setUsername() {
        PlayerPrefs.SetString("Name", enterUsername.text);
    }

    private void displayJoinCode()
    {
        joinCodeInput.text = "";
        joinCodeText.SetActive(true);
    }

    public void showMainMenu()
    {
        mainMenu.SetActive(true);
    }

    public void hideMainMenu()
    {
        mainMenu.SetActive(false);
    }
}