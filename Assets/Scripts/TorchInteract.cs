using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class TorchInteract : NetworkBehaviour, IInteractable
{
    public ItemData item;
    public TipoEquipo TipoEquipo;
    public Inventory inv;
    public InventoryUI invUi;
    public Canvas_Controller canvas;
    public ItemData newItem;
    public AudioSource interactSource;

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

    public void Start()
    {
        canvas = FindObjectOfType<Canvas_Controller>();
        newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.id = item.id;
        newItem.itemName = item.itemName;
        newItem.description = item.description;
        newItem.prefab = item.prefab;
        newItem.sizeX = item.sizeX;
        newItem.sizeY = item.sizeY;
        newItem.tipoEquipo = item.tipoEquipo;
        newItem.equipable = item.equipable;
        newItem.durabilidad = Mathf.Round(((int)(Random.Range(50, 75))));
        newItem.currentDurability = newItem.durabilidad;
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
                player.itemUpdated(newItem , true);
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
        StartCoroutine(DesactivateObject());
    }

    [ClientRpc]
    public void ApprovePickupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Player localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
        Inventory localInv = localPlayer.GetComponent<Inventory>();
        InventoryUI localInvUI = FindObjectOfType<InventoryUI>();
        ItemIconGenerator generator = FindObjectOfType<ItemIconGenerator>();

        if (newItem.icon == null && item.prefab != null)
        {
            newItem.icon = generator.GenerateIcon(newItem.prefab, newItem);
        }
        
        localInv.FillSpace(newItem);
        localInvUI.PlaceItem(newItem);
    }

    [ClientRpc]
    public void PlayInteractSoundClientRpc()
    {
        interactSource.Play();
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

    public IEnumerator DesactivateObject()
    {
        RequestDesactivateServerRpc();
        yield return new WaitForSeconds(interactSource.clip.length);
        RequestHideServerRpc();
    }
}