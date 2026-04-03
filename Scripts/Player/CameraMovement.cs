using UnityEngine;
using Unity.Netcode;

public class CameraMovement : NetworkBehaviour
{
    public PlayerData playerData;
    public Transform cameraTransform;

    private float xRotation = 0f;

    private NetworkVariable<float> networkYaw = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Start()
    {
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (cameraTransform != null)
            {
                cameraTransform.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleLocalLook();
        }
        else
        {
            ApplyRemoteLook();
        }
    }

    private void HandleLocalLook()
    {
        if (playerData.isInMenu)
            return;

        float mouseX = Input.GetAxis("Mouse X") * playerData.mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * playerData.mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        networkYaw.Value = transform.eulerAngles.y;
    }

    private void ApplyRemoteLook()
    {
        Vector3 euler = transform.eulerAngles;
        euler.y = networkYaw.Value;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }
}