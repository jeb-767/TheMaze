using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class ItemsDropInteract : NetworkBehaviour, IInteractable
{
    public ItemData item;
    public ItemData newItem;
    public AudioSource interactSource;

    public NetworkVariable<bool> visible = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> activated = new NetworkVariable<bool>(true);
    
    // 1. Añadimos la NetworkVariable para evitar la duplicación
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

    void Awake()
    {
        newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.id = item.id;
        newItem.itemName = item.itemName;
        newItem.description = item.description;
        newItem.prefab = item.prefab;
        newItem.sizeX = item.sizeX;
        newItem.sizeY = item.sizeY;
        newItem.tipoEquipo = item.tipoEquipo;
        newItem.equipable = item.equipable;
        newItem.quantity = item.quantity;
    }

    public void Start()
    {
        this.GetComponent<Animator>().SetBool("Floor", true);
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
        // Si sabemos localmente que ya se interactuó, salimos
        if (interacted.Value) return; 

        Inventory inv = player.GetComponent<Inventory>();
        InventoryUI invUI = FindObjectOfType<InventoryUI>();

        if (inv != null && invUI != null)
        {
            // Verificamos si tenemos el ítem (para apilar) o si hay espacio nuevo
            bool hasItemQuantity = invUI.CheckItemQuantity(newItem);
            
            if (hasItemQuantity || inv.CheckSpace(newItem))
            {
                // Pedimos permiso al servidor enviando nuestro ClientId
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
        // Validación en servidor: Si alguien ya lo cogió, ignoramos la petición
        if (interacted.Value) return;

        // Bloqueamos el objeto
        interacted.Value = true;

        // Preparamos el RPC solo para el jugador ganador
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };

        // Otorgamos el ítem
        ApprovePickupClientRpc(clientRpcParams);

        // Desactivamos para todos
        PlayInteractSoundClientRpc();
        StartCoroutine(DesactivateObject());
    }

    [ClientRpc]
    public void ApprovePickupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Esto solo corre en el cliente que recogió el objeto
        Player localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
        Inventory inv = localPlayer.GetComponent<Inventory>();
        InventoryUI invUI = FindObjectOfType<InventoryUI>();
        ItemIconGenerator generator = FindObjectOfType<ItemIconGenerator>();

        if (!invUI.CheckItemQuantity(newItem))
        {
            if (newItem.icon == null && newItem.prefab != null)
            {
                newItem.icon = generator.GenerateIcon(newItem.prefab, newItem);
            }
            inv.FillSpace(newItem);
            invUI.PlaceItem(newItem);
        }
        else
        {
            invUI.UpdateQuantityText(newItem);
        }
    }

    [ClientRpc]
    public void PlayInteractSoundClientRpc()
    {
        interactSource.Play();
    }

    public IEnumerator DesactivateObject()
    {
        RequestDesactivateServerRpc();
        this.GetComponent<Animator>().SetBool("Floor", false);
        yield return new WaitForSeconds(interactSource.clip.length);
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