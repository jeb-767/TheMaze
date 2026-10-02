using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class Armor_Interact : NetworkBehaviour, IInteractable
{
    public GameObject armor_part;
    public ItemData item;
    public TipoEquipo TipoEquipo;
    public ItemData newItem;
    private bool canInteract;
    public GameObject glow_blue, glow_red, glow_green, glow_yellow, glow_purple, glow_gray, grow;
    public Inventory inv;
    public InventoryUI invUi;
    public Canvas_Controller canvas;
    public AudioSource interactSource;

    public NetworkVariable<bool> visible = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> activated = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> interacted = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn() // <--- ASÍ DEBE SER
    {
        if (IsServer)
        {
            visible.Value = true;
            activated.Value = true;
        }
        visible.OnValueChanged += OnVisibleChanged;
        activated.OnValueChanged += OnActivatedChanged;
        // Forzamos la actualización visual nada más nacer para sincronizar a los que entran tarde
        SetVisibility(visible.Value);
        SetActivateComponents(activated.Value);
    }
    public override void OnNetworkDespawn()
    {
        visible.OnValueChanged -= OnVisibleChanged;
        activated.OnValueChanged -= OnActivatedChanged;
    }
    public void OnVisibleChanged(bool previousValue, bool newValue)
    {
        Debug.Log("Visibility changed from " + previousValue + " to " + newValue);
        SetVisibility(newValue);
    }
    public void OnActivatedChanged(bool previousValue, bool newValue)
    {
        SetActivateComponents(newValue);
    }
    void Awake()
    {
        GenerateItem();
    }
    public void SetVisibility(bool isVisible)
    {
        Debug.Log("Setting visibility to: " + isVisible);
        // 3. IMPORTANTE: Apagar todos los HIJOS (donde suele estar el modelo 3D real)
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

        // 2. Apagar collider del padre
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = activate;
        }
    }
    public void GenerateItem()
    {
        float rarityMult = 0;
        int probabilities = Random.Range(0, 100);
        newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.id = item.id;
        newItem.itemName = item.itemName;
        newItem.description = item.description;
        newItem.prefab = item.prefab;
        newItem.sizeX = item.sizeX;
        newItem.sizeY = item.sizeY;
        newItem.tipoEquipo = item.tipoEquipo;
        newItem.equipable = item.equipable;
        //newItem.itemName = transform.name;
        //newItem.equipable = true;
        if (probabilities > 98) //2%
        {
            newItem.rarezaEquipo = Rareza.Mhytic;
        }
        else if (probabilities > 93) //5%
        {
            newItem.rarezaEquipo = Rareza.Legendary;
        }
        else if (probabilities > 85) //8%
        {
            newItem.rarezaEquipo = Rareza.Epic;
        }
        else if (probabilities > 70)//15%
        {
            newItem.rarezaEquipo = Rareza.Super_rare;
        }
        else if (probabilities > 45)//25%
        {
            newItem.rarezaEquipo = Rareza.Rare;
        }
        else //45%
        {
            newItem.rarezaEquipo = Rareza.Common;
        }
        switch (newItem.rarezaEquipo)
        {
            case Rareza.Common:
                {
                    rarityMult = 1.0f;
                    break;
                }
            case Rareza.Rare:
                {
                    rarityMult = 1.2f;
                    break;
                }
            case Rareza.Super_rare:
                {
                    rarityMult = 1.4f;
                    break;
                }
            case Rareza.Epic:
                {
                    rarityMult = 1.6f;
                    break;
                }
            case Rareza.Legendary:
                {
                    rarityMult = 1.8f;
                    break;
                }
            case Rareza.Mhytic:
                {
                    rarityMult = 2.0f;
                    break;
                }

        }
        if (item.tipoEquipo == TipoEquipo.Weapon)
        {
            newItem.Daño = Mathf.Round(((float)(Random.Range(6, 12)) * rarityMult) * 100f) / 100f;
        }
        else
        {
            newItem.Armadura = Mathf.Round(((float)(Random.Range(2, 7) / 10f) * rarityMult) * 100f) / 100f;
        }
        newItem.durabilidad = Mathf.Round(((int)(Random.Range(50, 75)) * rarityMult) * 100f) / 100f;

        newItem.currentDurability = newItem.durabilidad;
        newItem.icon = null;
        interactSource = this.GetComponent<AudioSource>();
    }
    public void Start()
    {

        /*glow_gray.SetActive(false);
        glow_yellow.SetActive(false);
        glow_blue.SetActive(false);
        glow_red.SetActive(false);
        glow_green.SetActive(false);
        glow_purple.SetActive(false);
        if(newItem.rarezaEquipo == Rareza.common)
        {
            grow = Instantiate(glow_gray , this.transform.position, Quaternion.identity ,transform);
        }
        else if(newItem.rarezaEquipo == Rareza.rare)
        {
            grow = Instantiate(glow_green , this.transform.position, Quaternion.identity , transform);

        }
        else if(newItem.rarezaEquipo == Rareza.super_rare)
        {
            grow = Instantiate(glow_blue , this.transform.position, Quaternion.identity , transform);

        }
        else if(newItem.rarezaEquipo == Rareza.epic)
        {
            grow = Instantiate(glow_purple , this.transform.position, Quaternion.identity , transform);

        }
        else if(newItem.rarezaEquipo == Rareza.legendary)
        {
            grow = Instantiate(glow_yellow , this.transform.position, Quaternion.identity , transform);

        }
        else if(newItem.rarezaEquipo == Rareza.mhytic)
        {
            grow = Instantiate(glow_red , this.transform.position, Quaternion.identity , transform);

        }*/
    }
    public void Interact(Player player)
    {
        if(interacted.Value)
        {
            return;
        }
        canInteract = false;
        inv = player.inventory;
        canvas = player.canvas;
        invUi = player.canvas.ui;
        if (inv != null && invUi != null)
        {
            if (inv.CheckSpace(newItem))
            {
                RequestPickupServerRpc(NetworkManager.Singleton.LocalClientId);
                player.itemUpdated(newItem , true);
            }
        }
        /*if (canInteract)
        {
            interacted.Value = true;
            this.GetComponent<Animator>().SetBool("Floor", false);
            Debug.Log("Picked up armor part");
            if (!IsSpawned)
            {
                Debug.LogError("¡ERROR CRÍTICO! Este objeto no está Spawneado en la red. El RPC fallará.");
                return;
            }
            RequestInteractSoundServerRpc();
            StartCoroutine(DesactivateObject());
            inv.FillSpace(newItem);
            Debug.Log("Filled space");
            invUi.PlaceItem(newItem);
            Debug.Log("Plaeced item");
        }*/
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(ulong clientId)
    {
        // El SERVIDOR verifica si alguien más ya lo tomó. 
        // Esto evita la duplicación (Condición de carrera resuelta).
        if (interacted.Value) 
        {
            return; 
        }

        // El servidor bloquea el objeto para los demás
        interacted.Value = true;

        // Preparamos un mensaje que SOLO irá al cliente que ganó el objeto
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };

        // Le decimos al cliente ganador que añada el ítem
        ApprovePickupClientRpc(clientRpcParams);

        // Disparamos los efectos visuales/sonoros para todos y empezamos a ocultar el objeto
        PlayInteractSoundClientRpc();
        StartCoroutine(DesactivateObject()); 
    }
    [ClientRpc]
    public void ApprovePickupClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Esta lógica SOLO se ejecutará en la pantalla del jugador que se llevó el objeto
        Player localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().GetComponent<Player>();
        
        ItemIconGenerator generator = FindObjectOfType<ItemIconGenerator>();
        if (newItem.icon == null && newItem.prefab != null)
        {
            newItem.icon = generator.GenerateIcon(newItem.prefab, newItem);
        }

        // Añadimos al inventario local
        localPlayer.inventory.FillSpace(newItem);
        localPlayer.canvas.ui.PlaceItem(newItem);
        this.GetComponent<Animator>().SetBool("Floor", false);
        
        Debug.Log("Objeto recogido exitosamente con validación del servidor");
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestInteractSoundServerRpc()
    {
        // 2. El servidor recibe el aviso y le ordena a TODOS los clientes que hagan ruido
        PlayInteractSoundClientRpc();
    }

    // 3. Esta función se ejecuta en la computadora de cada uno de los jugadores
    [ClientRpc]
    public void PlayInteractSoundClientRpc()
    {
        interactSource.Play();
    }
    public void Desapear()
    {
        if (canInteract)
        {
            RequestHideServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestHideServerRpc()
    {
        Debug.Log("Hiding armor part on server");
        visible.Value = false;
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestDesactivateServerRpc()
    {
        Debug.Log("Hiding armor part on server");
        activated.Value = false;
    }
    public IEnumerator DesactivateObject()
    {
        RequestDesactivateServerRpc();
        yield return new WaitForSeconds(interactSource.clip.length);
        RequestHideServerRpc();
    }
}