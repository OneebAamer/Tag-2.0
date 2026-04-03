using UnityEngine;

public class MenuBehaviour : MonoBehaviour
{
    public PlayerData playerData;
    public CameraMovement cameraMovement;
    public GameObject menuCanvas;

    public bool isInThisMenu = false;
    private int openedFrame = -1;

    private void Update()
    {
        // prevent closing on the same frame it was opened
        if (Time.frameCount == openedFrame) return;
        if (Input.GetKeyDown(KeyCode.Escape) && isInThisMenu)
        {
            closeMenu();
        }
    }

    public void openMenu()
    {
        openedFrame = Time.frameCount;
        toggleMenuBehaviour(true);
    }

    public void closeMenu()
    {
        toggleMenuBehaviour(false);
    }

    public void setInMenu()
    {
        isInThisMenu = true;
    }

    public void setNotInMenu()
    {
        isInThisMenu = false;
    }

    void toggleMenuBehaviour(bool flag)
    {
        menuCanvas.SetActive(flag);
        Cursor.visible = flag;
        Cursor.lockState = flag ? CursorLockMode.None : CursorLockMode.Locked;
        cameraMovement.enabled = !flag;
        playerData.isInMenu = flag;
        isInThisMenu = flag;
    }
}
