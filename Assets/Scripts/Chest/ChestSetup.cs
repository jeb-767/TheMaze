using UnityEngine;
using Unity.Netcode;
using TMPro;
public class ChestSetup : NetworkBehaviour ,IInteractable
{
    public ItemDataBase itemDataBase;
    public NetworkVariable<int> numItems = new NetworkVariable<int>(0);
    public NetworkList<NetworkItemData> generatedItems = new NetworkList<NetworkItemData>();
    public ItemData newItem;
    public ItemData item;
    public ItemIconGenerator imageGenerator;
    public TextMeshProUGUI textQuantity;

    public override void OnNetworkDespawn()
    {
        // Desuscribirse para evitar errores de memoria
        generatedItems.OnListChanged -= OnChestItemsChanged;
    }
    // Reemplaza el void Start() por OnNetworkSpawn
    public override void OnNetworkSpawn()
    {
        // Suscribirse a los cambios de lista como vimos antes
        generatedItems.OnListChanged += OnChestItemsChanged;

        imageGenerator = FindObjectOfType<ItemIconGenerator>();

        // SOLO EL SERVIDOR DECIDE QUÉ HAY EN EL COFRE
        if (IsServer)
        {
            GenerateDefaultItem();
            int prob = Random.Range(0, 100);
            switch (prob)
            {
                case < 5:
                    numItems.Value = 5;
                    break;
                case < 15:
                    numItems.Value = 4;
                    break;
                case < 30:
                    numItems.Value = 3;
                    break;
                case < 50:
                    numItems.Value = 2;
                    break;
                default:
                    numItems.Value = 1;
                    break;
            }

            for (int i = 0; i < numItems.Value; i++)
            {
                int randomItemId = itemDataBase.GetRandomItem();
                item = itemDataBase.objects[randomItemId];
                
                switch (item.tipoEquipo)
                {
                    case TipoEquipo.Weapon:
                        GenerateItem();
                        break;
                    case TipoEquipo.Tourch:
                        GenerateItemTorch();
                        break;
                    case TipoEquipo.Dropped:
                    case TipoEquipo.Key:
                        GenerateItemDropped();
                        break;
                    default:
                        GenerateItem();
                        break;
                }
                
                NetworkItemData Netitem = GenerateItems();
                generatedItems.Add(Netitem); // Al añadirlo aquí, se sincroniza automáticamente a todos
            }
        }
    }
    public void GenerateDefaultItem()
    {
        newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.id = 0;
        newItem.itemName = "Default Item";
        newItem.description = "Default item description";
        newItem.prefab = null;
        newItem.sizeX = 1;
        newItem.sizeY = 1;
        newItem.tipoEquipo = TipoEquipo.Dropped;
        newItem.equipable = false;
        newItem.rarezaEquipo = Rareza.Common;
        newItem.Daño = 0f;
        newItem.Armadura = 0f;
        newItem.durabilidad = 0f;
        newItem.currentDurability = 0f;
    }
    public NetworkItemData GenerateItems()
    {
        NetworkItemData netItem = new NetworkItemData
        {
            id = newItem.id,
            tipoEquipo = newItem.tipoEquipo,
            rarezaEquipo = newItem.rarezaEquipo,
            Daño = newItem.Daño,
            Armadura = newItem.Armadura,
            durabilidad = newItem.durabilidad,
            currentDurability = newItem.currentDurability,
            quantity = newItem.quantity
        };
        return netItem;
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
        newItem.icon = imageGenerator.GenerateIcon(item.prefab, item);
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
    }
    public void GenerateItemTorch()
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
        newItem.rarezaEquipo = Rareza.Common;
        newItem.durabilidad = 100f;
        newItem.currentDurability = 100f;
        newItem.icon = imageGenerator.GenerateIcon(item.prefab, item);
    }
    public void GenerateItemDropped()
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
        newItem.rarezaEquipo = Rareza.Common;
        newItem.durabilidad = 0f;
        newItem.currentDurability = 0f;
        newItem.icon = imageGenerator.GenerateIcon(item.prefab, item);
    }
    // Método para añadir (el que ya tenías o ibas a poner)
    [ServerRpc(RequireOwnership = false)]
    public void AddItemToChestServerRpc(NetworkItemData newItemData)
    {
        if (generatedItems.Count < 5)
        {
            generatedItems.Add(newItemData);
            numItems.Value = generatedItems.Count;
        }
    }

    // NUEVO: Método para quitar objetos del cofre
    [ServerRpc(RequireOwnership = false)]
    public void RemoveItemFromChestServerRpc(int itemId, float durabilidad)
    {
        // Buscamos el objeto en la lista para eliminarlo
        for (int i = 0; i < generatedItems.Count; i++)
        {
            // Comparamos por ID y durabilidad para asegurarnos de quitar el correcto
            if (generatedItems[i].id == itemId && generatedItems[i].currentDurability == durabilidad)
            {
                generatedItems.RemoveAt(i);
                numItems.Value = generatedItems.Count;
                break;
            }
        }
    }

    // NUEVO: Método centralizado para actualizar la UI del cofre
    // NUEVO: Método centralizado para actualizar la UI del cofre
    public void RefreshChestUI(Player player)
    {
        if (player.currentOpenChest != this) return;

        // 1. Limpiamos los objetos antiguos de la UI AQUÍ, antes de crear los nuevos
        foreach (Transform child in player.ui.itemsChestParent)
        {
            if(!child.CompareTag("ChestUI"))
            {
                // ¡LÍNEA NUEVA! Borramos el objeto de la lista de colisiones del inventario
                player.ui.DeleteItemRT(child.GetComponent<RectTransform>());
                
                Destroy(child.gameObject);
            }
        }
        player.canvas.chestText.text = generatedItems.Count.ToString();
        // 2. Reconstruimos todos los objetos basándonos en la lista actual del servidor
        foreach (NetworkItemData netItem in generatedItems)
        {
            ItemData reconstructedItem = ScriptableObject.CreateInstance<ItemData>();
            ItemData baseItem = itemDataBase.GetItemByID(netItem.id);
            
            reconstructedItem.id = netItem.id;
            reconstructedItem.itemName = baseItem.itemName;
            reconstructedItem.description = baseItem.description;
            reconstructedItem.prefab = baseItem.prefab;
            reconstructedItem.sizeX = baseItem.sizeX;
            reconstructedItem.sizeY = baseItem.sizeY;
            reconstructedItem.equipable = baseItem.equipable;
            
            reconstructedItem.tipoEquipo = netItem.tipoEquipo;
            reconstructedItem.rarezaEquipo = netItem.rarezaEquipo;
            reconstructedItem.Daño = netItem.Daño;
            reconstructedItem.Armadura = netItem.Armadura;
            reconstructedItem.durabilidad = netItem.durabilidad;
            reconstructedItem.currentDurability = netItem.currentDurability;
            reconstructedItem.quantity = netItem.quantity;

            reconstructedItem.icon = imageGenerator.GenerateIcon(baseItem.prefab, baseItem);

            player.ui.PlaceItemChest(reconstructedItem);
        }
    }

    // Actualizamos Interact para usar el nuevo método
    public void Interact(Player player)
    {
        player.currentOpenChest = this;
        // 1. Limpiamos los objetos antiguos de la UI
        
        RefreshChestUI(player);
        player.canvas.OpenChestPanel();
    }

    // Actualizamos el evento de red para que reaccione tanto al añadir como al quitar
    private void OnChestItemsChanged(NetworkListEvent<NetworkItemData> changeEvent)
    {
        // 1. Verificamos que estamos conectados como cliente y el SpawnManager está listo
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && NetworkManager.Singleton.SpawnManager != null)
        {
            // 2. Obtenemos el objeto del jugador local de forma segura
            NetworkObject localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            
            // 3. Comprobamos que el jugador ya existe físicamente en el mundo
            if (localPlayerObj != null)
            {
                Player localPlayer = localPlayerObj.GetComponent<Player>();
                
                // 4. Si el jugador existe y además tiene este cofre específico abierto, recargamos la UI
                if (localPlayer != null && localPlayer.currentOpenChest == this)
                {
                    RefreshChestUI(localPlayer); 
                }
            }
        }
}
}
