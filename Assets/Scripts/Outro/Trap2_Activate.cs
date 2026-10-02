using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Trap2_Activate : NetworkBehaviour, IInteractable
{
    Animator anim;

    public AudioSource auidio;
    public BoxCollider colider;
    public BoxCollider colider2;
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
    void Start()
    {
        anim = GetComponent<Animator>();
        colider.enabled = false;
    }

    public void Interact(Player player)
    {
        ActivateTrapServerRpc(true); // Llamamos al RPC para activar la trampa desde el servidor
    }
    [ServerRpc(RequireOwnership = false)]
    private void ActivateTrapServerRpc(bool val)
    {
        isActivated.Value = val; // Cambia el estado a activado en el servidor
        if (val)
        {
            StartCoroutine(Activate_Trap());
        }
    }
    private void OnIsActivatedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            // Aquí puedes agregar cualquier lógica adicional que quieras ejecutar cuando la trampa se active
            colider2.enabled = false;
            return;
        }
    }
    private IEnumerator Activate_Trap()
    {
        anim.SetTrigger("Activate");
        yield return new WaitForSeconds(0.75f);
        colider.enabled = true;
        yield return new WaitForSeconds(3f);
        colider.enabled = false;
        anim.SetTrigger("Desactivate");
        colider2.enabled = true;
        ActivateTrapServerRpc(false);
    }
}
