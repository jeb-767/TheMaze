using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class Trap_Interact_1 : NetworkBehaviour, IInteractable
{
    public AudioSource auidio;
    public void Interact(Player player)
    {
        Debug.Log("Trap_Interact_1");
        player.Trap1();
    }
    [ServerRpc(RequireOwnership = false)]
    private void ActivateTrapServerRpc(bool val)
    {
        auidio.Play();
    }
}
