using UnityEngine;
using System.Collections;
using Unity.Netcode;

public enum OrbType
{
    Jump,
}

public class Orb : NetworkBehaviour
{
    [Header("Orb Settings")]
    public OrbType orbType = OrbType.Jump;
    public float effectAmount = 10f;
    public float effectDuration = 5f;
    public float respawnTime = 10f;

    private Collider orbCollider;
    private Renderer orbRenderer;

    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(true);

    private void Awake()
    {
        orbCollider = GetComponent<Collider>();
        orbRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        isActive.OnValueChanged += (oldValue, newValue) =>
        {
            orbRenderer.enabled = newValue;
            orbCollider.enabled = newValue;
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (!isActive.Value) return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            HandlePickup(player);
        }
    }

    private void HandlePickup(PlayerMovement player)
    {
        Debug.Log($"Player {player.name} picked up a {orbType} orb!");

        switch (orbType)
        {
            case OrbType.Jump:
                player.ApplyJumpBoost(effectAmount, effectDuration);
                break;
        }

        StartCoroutine(TemporarilyDisableOrb());
    }

    private IEnumerator TemporarilyDisableOrb()
    {
        if (!IsServer) yield break;

        isActive.Value = false;

        yield return new WaitForSeconds(respawnTime);

        isActive.Value = true;

        Debug.Log($"{orbType} orb is now active again!");
    }
}