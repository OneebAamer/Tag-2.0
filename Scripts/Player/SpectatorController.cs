using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SpectatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera spectatorCamera;
    [SerializeField] private AudioListener spectatorAudioListener;
    [SerializeField] private PlayersManager playersManager;
    [SerializeField] private TMP_Text spectatingInfo;

    [Header("Options")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    private PlayerData localPlayer;
    private readonly List<PlayerData> aliveTargets = new();
    private int currentIndex = -1;
    private bool isSpectating;

    private void Awake()
    {
        SetCameraState(false);
    }

    public void SetSpectating(PlayerData localOwner, bool value)
    {
        Debug.Log("SPECTATINGGGG");
        localPlayer = localOwner;
        isSpectating = value;

        SetCameraState(value);

        if (!value)
        {
            currentIndex = -1;
            aliveTargets.Clear();
            return;
        }

        RefreshTargets();
        PickFirstTarget();
    }

    private void Update()
    {
        if (!isSpectating)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            CycleTarget(-1);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CycleTarget(1);
        }

        if (!HasValidCurrentTarget())
        {
            RefreshTargets();
            PickFirstTarget();
        }
    }

    private void LateUpdate()
    {
        if (!isSpectating)
            return;

        PlayerData target = GetCurrentTarget();
        if (target == null)
            return;

        Transform anchor = target.SpectateAnchor;
        spectatorCamera.transform.SetPositionAndRotation(
            anchor.position + positionOffset,
            anchor.rotation
        );
    }

    private void SetCameraState(bool enabledState)
    {
        if (spectatorCamera != null)
            spectatorCamera.enabled = enabledState;

        if (spectatorAudioListener != null)
            spectatorAudioListener.enabled = enabledState;
    }

    private void RefreshTargets()
    {
        aliveTargets.Clear();

        aliveTargets.AddRange(
            playersManager.GetAllAlivePlayers()
                .Where(p => localPlayer == null || p.OwnerClientId != localPlayer.OwnerClientId)
        );

        if (aliveTargets.Count == 0)
        {
            currentIndex = -1;
        }
        else if (currentIndex >= aliveTargets.Count)
        {
            currentIndex = 0;
        }
    }

    private void PickFirstTarget()
    {
        currentIndex = aliveTargets.Count > 0 ? 0 : -1;
    }

    private void CycleTarget(int direction)
    {
        RefreshTargets();

        if (aliveTargets.Count == 0)
        {
            currentIndex = -1;
            return;
        }

        if (currentIndex < 0)
        {
            currentIndex = 0;
            return;
        }

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = aliveTargets.Count - 1;
        else if (currentIndex >= aliveTargets.Count)
            currentIndex = 0;
    }

    private bool HasValidCurrentTarget()
    {
        PlayerData target = GetCurrentTarget();
        return target != null && !target.IsSpectating.Value;
    }

    private PlayerData GetCurrentTarget()
    {
        if (currentIndex < 0 || currentIndex >= aliveTargets.Count)
            return null;

        return aliveTargets[currentIndex];
    }

    public void GetSpectatingName()
    {
        PlayerData target = GetCurrentTarget();
        spectatingInfo.text = target.Username.Value.ToString();
    }
}