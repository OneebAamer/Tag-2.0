using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class PlayerVisuals : NetworkBehaviour
{
    [Header("Visuals")]
    [SerializeField] public Material taggedMaterial;
    [SerializeField] public Material untaggedMaterial;
    [SerializeField] public GameObject playerBody;
    [SerializeField] public CharacterController playerHitbox;
    [SerializeField] public PostProcessLayer vignette;
    [SerializeField] public LayerMask taggedLayer;
    [SerializeField] public LayerMask untaggedLayer;

    public MeshRenderer bodyRenderer;

    public void ApplyTagVisuals(bool isTagged)
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.material = isTagged ? taggedMaterial : untaggedMaterial;
        }

        if (vignette != null)
        {
            vignette.volumeLayer = isTagged ? taggedLayer : untaggedLayer;
        }
    }

    public void ApplySpectatorVisuals(bool isSpectating)
    {
        playerHitbox.enabled = !isSpectating;
        bodyRenderer.gameObject.SetActive(!isSpectating);
    }
}