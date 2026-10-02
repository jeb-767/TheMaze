using UnityEngine;
using Unity.Netcode;

public abstract class NPC_Talk : NetworkBehaviour, IInteractable
{
    public Animator anim;
    public AudioSource npc_audio;
    protected Player player;

    // Lógica común de interacción
    public virtual void Interact(Player player)
    {
        this.player = player;
        player.canvas.npc = this.gameObject;
        
        // Llamada de red común para el sonido
        //RequestPlayTalkSoundServerRpc();
    }

    // Métodos abstractos que obligan a cada NPC hijo a definir qué pasa al darle a Yes/No
    public abstract void OnYes();
    public abstract void OnNo();

    protected void UpdateAnimations(GameObject Talk_Panel)
    { 
        // Lógica común de animación[cite: 2]
        if (player != null && player.canvas.npc == this.gameObject)
        {
            anim.SetBool("Conversation", Talk_Panel.activeInHierarchy);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayTalkSoundServerRpc()
    {
        PlayTalkSoundClientRpc(); //[cite: 2]
    }

    [ClientRpc]
    public void PlayTalkSoundClientRpc()
    {
        if (npc_audio != null)
        {
            npc_audio.Play(); //[cite: 2]
        }
    }
}