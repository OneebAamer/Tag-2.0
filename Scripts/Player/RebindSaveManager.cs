using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;
    private const string RebindsKey = "input-rebinds";

    private void Awake()
    {
        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        if (actions == null)
            return;

        string json = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindsKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (actions == null)
            return;

        if (!PlayerPrefs.HasKey(RebindsKey))
            return;

        string json = PlayerPrefs.GetString(RebindsKey);
        actions.LoadBindingOverridesFromJson(json);
    }

    public void ResetAll()
    {
        if (actions == null)
            return;

        foreach (var map in actions.actionMaps)
        {
            foreach (var action in map.actions)
                action.RemoveAllBindingOverrides();
        }

        PlayerPrefs.DeleteKey(RebindsKey);
    }
}