using UnityEngine;

public class SaveKeybinds : MonoBehaviour
{
    [SerializeField] private RebindSaveManager rebindSaveManager;

    private void OnDisable()
    {
        rebindSaveManager.Save();
    }
}
