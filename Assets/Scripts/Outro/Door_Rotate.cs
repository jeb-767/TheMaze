using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Netcode;

public class Door_Rotate : NetworkBehaviour, IInteractable
{
    public GameObject Transform;
    public Animator anim;
    public NetworkVariable<bool> is_opened = new NetworkVariable<bool>(false);
    public string color;
    private Key script;
    AudioSource auidio;
    public List<AudioClip> auidos = new List<AudioClip>(); //0 = abrir , 1 = cerrar, 2 = no poder abrir(solo en color)
    int Control_sound = 0;
    public MeshRenderer_Hide meshRendererHide;
    public override void OnNetworkSpawn()
    {
        auidio = GetComponent<AudioSource>();
        is_opened.Value = false; 
        if (IsServer)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.forward, out hit, 1f))
            {
                if (hit.transform.tag == "Wall")
                {
                    Transform.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }
        }
        is_opened.OnValueChanged += OnOpenChange; // Suscribimos al cambio de vida
    }
    public override void OnNetworkDespawn()
    {
        is_opened.OnValueChanged -= OnOpenChange; // Desuscribimos al despawn
    }
    public void Interact(Player player)
    {
        Control_sound = 0;
        if (color != "No Color")
        {
            foreach (ItemData a in player.inventory.items)
            {
                //script = a.GetComponent<Key>();
                if (a.tipoEquipo == TipoEquipo.Key && a.itemName.Contains(this.color))
                {
                    ToggleDoorServerRpc();
                    Control_sound = 1;
                }
            }
            if (Control_sound == 0)
            {
                RequestTryOpenSoundServerRpc();
            }
        }
        else
        {
            ToggleDoorServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)] // Permite que cualquier cliente pida el cambio
    public void ToggleDoorServerRpc()
    {
        // Como esto se ejecuta en el servidor, SÍ tiene permiso de escritura
        is_opened.Value = !is_opened.Value;
    }
    private void OnOpenChange(bool previousValue, bool newValue)
    {
        if(newValue)
        {
            anim.SetTrigger("Open");
            auidio.clip = auidos[0];
            auidio.Play();
        }
        else
        {
            anim.SetTrigger("Close");
            StartCoroutine(WaitForClose());
        }
    }
    private IEnumerator WaitForClose()
    {
        yield return new WaitForSeconds(1f);
        auidio.clip = auidos[1];
        auidio.Play();
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestTryOpenSoundServerRpc()
    {
        // 2. El servidor recibe el aviso y le ordena a TODOS los clientes que hagan ruido
        PlayTryOpenSoundClientRpc();
    }

    // 3. Esta función se ejecuta en la computadora de cada uno de los jugadores
    [ClientRpc]
    public void PlayTryOpenSoundClientRpc()
    {
        auidio.clip = auidos[2];
        auidio.Play();
    }
}
