using UnityEngine;
using Unity.Netcode;
public class Trap_2_Spike_Damage : NetworkBehaviour, IInteractable
{
    public AudioSource auidio;
    public Collider colider;
    public NetworkVariable<bool> isActivated = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isActivated.Value = false; // Aseguramos que el valor inicial se sincronice
        }
        isActivated.OnValueChanged += OnIsActivatedChanged; // Suscribimos al cambio de estado
    }
    public override void OnNetworkDespawn()
    {
        isActivated.OnValueChanged -= OnIsActivatedChanged; // Desuscribimos al despawn
    }
    public void Interact(Player player)
    {
        player.Trap2();
        ActivateTrapServerRpc(true); // Llamamos al RPC para activar la trampa desde el servidor
        colider.enabled = false;
    }
    [ServerRpc(RequireOwnership = false)]
    private void ActivateTrapServerRpc(bool val)
    {
        isActivated.Value = val; // Cambia el estado a activado en el servidor
    }
    private void OnIsActivatedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            auidio.Play();
            ActivateTrapServerRpc(false);
            return;
        }
    }
}
