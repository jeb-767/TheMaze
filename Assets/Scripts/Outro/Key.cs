using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Key : NetworkBehaviour, IInteractable
{
    public MeshRenderer PLS;
    public string color;
    public Collider keyCollider; // Renombrado de 'collider' a 'keyCollider' para evitar conflictos de hiding con el componente obsoleto
    AudioSource audioSource; // Renombrado de 'auidio' por claridad
    public ItemData item;
    
    public NetworkVariable<bool> visible = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> activated = new NetworkVariable<bool>(true);
    
    // Variable de red añadida
    public NetworkVariable<bool> interacted = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            visible.Value = true;
            activated.Value = true;
        }
        visible.OnValueChanged += OnVisibleChanged;
        activated.OnValueChanged += OnActivatedChanged;
        SetVisibility(visible.Value);
        SetActivateComponents(activated.Value);
    }

    public override void OnNetworkDespawn()
    {
        visible.OnValueChanged -= OnVisibleChanged;
        activated.OnValueChanged -= OnActivatedChanged;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PLS.enabled = true;
        keyCollider.enabled = true;
    }

    public void OnVisibleChanged(bool previousValue, bool newValue)
    {
        SetVisibility(newValue);
    }

    public void OnActivatedChanged(bool previousValue, bool newValue)
    {
        SetActivateComponents(newValue);
    }

    public void SetVisibility(bool isVisible)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(isVisible);
        }
    }

    public void SetActivateComponents(bool activate)
    {
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().enabled = activate;
        }
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = activate;
        }
    }

    public void Interact(Player player)
    {
        if (interacted.Value) return;

        Inventory localInv = player.GetComponent<Inventory>();

        if (localInv != null)
        {
            if (localInv.CheckSpace(item))
            {
                RequestPickupServerRpc(NetworkManager.Singleton.LocalClientId);
                player.itemUpdated(item , true);
            }
            else
            {
                Debug.Log("Inventario lleno");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(ulong clientId)
    {
        if (interacted.Value) return;

        interacted.Value = true;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };

        ApprovePickupClientRpc(clientRpcParams);

        PlayInteractSoundClientRpc();
        StartCoroutine(DesactivateKey());
    }

    [ClientRpc]
    public void ApprovePickupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Player localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
        Inventory localInv = localPlayer.GetComponent<Inventory>();
        InventoryUI localInvUI = FindObjectOfType<InventoryUI>();
        ItemIconGenerator generator = FindObjectOfType<ItemIconGenerator>();

        keyCollider.enabled = false;
        PLS.enabled = false;

        if (item.icon == null && item.prefab != null)
        {
            item.icon = generator.GenerateIcon(item.prefab, item);
        }
        localInv.FillSpace(item);
        localInvUI.PlaceItem(item);
    }

    [ClientRpc]
    public void PlayInteractSoundClientRpc()
    {
        audioSource.Play();
    }

    public IEnumerator DesactivateKey()
    {
        RequestDesactivateServerRpc();
        yield return new WaitForSeconds(audioSource.clip.length);
        RequestHideServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHideServerRpc()
    {
        visible.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDesactivateServerRpc()
    {
        activated.Value = false;
    }
}