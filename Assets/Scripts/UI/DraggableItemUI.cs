using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class DraggableItemUI : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    public Transform originalParent;
    public RectTransform inventoryPanel; // Asignar en el inspector (contenedor del inventario)
    public Inventory inventory;
    public ItemData itemData; // Asigna esto al instanciar el item UI
    public string startDrag = "Normal";
    private EquipmentSlot equipmenSlot;
    private ReinfrocementSlot reinfrocementSlot;
    private FragmentSlot fragmentSlot;
    public InventoryUI uiInventory;
    public GameObject descriptionPanel;
    public GameObject NamePanel;
    public Image item_icon;
    public TextMeshProUGUI name1;
    public TextMeshProUGUI name2;
    public TextMeshProUGUI description;
    public TextMeshProUGUI type;
    public TextMeshProUGUI other;
    public TextMeshProUGUI quality;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI durabilityText;
    public Slider durability1, durability2;
    public int quantity = 0;
    private bool Open_Description = false;
    public bool equiped = false;
    public GameObject Player;
    private bool newItem;
    public int typeSlot = 0; //0 -> equipment slot, 1 -> reinforcement slot ,2 -> fragment slot
    public AudioSource destroySource;

    [SerializeField] private GameObject newItemIndicator;
    [SerializeField] public Canvas_Controller canvas;
    public bool isDragged;
    public GameObject itemDrop;

    public void Awake()
    {
        canvas = GameObject.FindWithTag("UI").GetComponent<Canvas_Controller>();

    }
    public void Start()
    {
        isDragged = false;
        /*uiInventory = GameObject.FindWithTag("UI").GetComponent<InventoryUI>();
        Player = GameObject.FindWithTag("Player");
        inventory = Player.GetComponent<Inventory>();*/
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        inventoryPanel = transform.parent.GetComponent<RectTransform>();
        descriptionPanel.SetActive(false);
        name1.text = itemData.itemName;
        name2.text = itemData.itemName;
        description.text = itemData.description;
        type.text = "Slot: " + itemData.tipoEquipo.ToString();
        quality.text = "Quality: " + itemData.rarezaEquipo.ToString();
        item_icon.sprite = itemData.icon;
        UpdateQuantity();
        if (itemData.tipoEquipo == TipoEquipo.Weapon)
        {
            other.text = "Damage: " + itemData.Daño.ToString();
        }
        else
        {
            other.text = "Armor: " + itemData.Armadura.ToString();
        }
        NamePanel.SetActive(false);
        if (itemData.tipoEquipo == TipoEquipo.Key || itemData.tipoEquipo == TipoEquipo.Dropped)
        {
            durability1.gameObject.SetActive(false);
            durability2.gameObject.SetActive(false);
            durabilityText.gameObject.SetActive(false);
        }
        else
        {
            SetupDurability();
        }
    }
    public void Update()
    {
        if ((rectTransform.anchoredPosition.x >= 1350 || rectTransform.anchoredPosition.x <= 0 || rectTransform.anchoredPosition.y >= 0f || rectTransform.anchoredPosition.y <= -830f) && isDragged == false && equiped == false)
        {
            leave(false);
        }
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player");
            inventory = Player.GetComponent<Inventory>();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Open_Description)
        {
            descriptionPanel.SetActive(false);
            Open_Description = false;
        }
        else if (!Open_Description)
        {
            descriptionPanel.SetActive(true);
            Open_Description = true;
            rectTransform.SetAsLastSibling();
        }
        if (eventData.clickCount == 2)
        {
            Debug.Log("¡Doble Clic detectado en el objeto!");
            // Tu lógica aquí (ej: abrir un cofre, seleccionar unidad)
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // El ratón entra en el objeto
        NamePanel.SetActive(true);
        if (newItem = true)
        {
            newItemIndicator.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // El ratón sale del objeto
        NamePanel.SetActive(false);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        Debug.Log(originalParent);
        transform.SetParent(inventoryPanel); // mover al nivel del panel, para que no quede limitado por slots
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // lo ves semitransparente al arrastrar
        uiInventory.currentDraggedItem = this.rectTransform;
        isDragged = true;
        descriptionPanel.SetActive(false);
        Open_Description = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //rectTransform.position += eventData.delta; // mover con el ratón
        rectTransform.position = Input.mousePosition; // mover con el ratón
        isDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        uiInventory.currentDraggedItem = null;

        // Si el item se suelta fuera de su panel original (ej. sale del cofre)
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, Input.mousePosition, eventData.pressEventCamera))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            bool droppedInValidArea = false; // Variable para saber si debemos evitar que caiga al suelo

            foreach (RaycastResult result in results)
            {
                // ==========================================
                // CASO 1: Soltamos en el INVENTARIO PRINCIPAL
                // ==========================================
                if (result.gameObject.CompareTag("MainInventoryUI"))
                {
                    // Guardamos si el objeto viene del cofre
                    bool comesFromChest = (originalParent == uiInventory.itemsChestParent);

                    // Si viene del cofre, comprobamos si hay espacio
                    if (comesFromChest)
                    {
                        if (!inventory.CheckSpace(itemData))
                        {
                            Debug.Log("No hay suficiente espacio en el inventario.");
                            transform.SetParent(originalParent);
                            droppedInValidArea = true;
                            break;
                        }
                    }

                    // 1. Primero cambiamos el padre al inventario para salvar el objeto de la destrucción
                    inventoryPanel = uiInventory.itemsParent.GetComponent<RectTransform>();
                    transform.SetParent(inventoryPanel);
                    originalParent = inventoryPanel;

                    // 2. Procesamos la lógica y avisamos al servidor
                    if (comesFromChest)
                    {
                        inventory.FillSpace(itemData);
                        inventory.AddItem(itemData);
                        uiInventory.AddItemRT(rectTransform);

                        ChestSetup chest = canvas.player.currentOpenChest;
                        if (chest != null)
                        {
                            chest.RemoveItemFromChestServerRpc(itemData.id, itemData.currentDurability);
                        }
                    }
                    
                    droppedInValidArea = true;
                    break;
                }
                
                // ==========================================
                // CASO 2: Soltamos en el COFRE
                // ==========================================
                if (result.gameObject.CompareTag("ChestUI"))
                {
                    ChestSetup chest = canvas.player.currentOpenChest;

                    if (chest != null && chest.generatedItems.Count < 5)
                    {
                        NetworkItemData netItem = new NetworkItemData
                        {
                            id = itemData.id,
                            tipoEquipo = itemData.tipoEquipo,
                            rarezaEquipo = itemData.rarezaEquipo,
                            Daño = itemData.Daño,
                            Armadura = itemData.Armadura,
                            durabilidad = itemData.durabilidad,
                            currentDurability = itemData.currentDurability,
                            quantity = itemData.quantity
                        };

                        // Enviamos el objeto al servidor para que lo guarde
                        chest.AddItemToChestServerRpc(netItem);

                        // Destruimos el objeto visual local del inventario
                        inventory.capacity -= itemData.sizeX * itemData.sizeY;
                        canvas.capacity1.text = inventory.capacity.ToString();
                        inventory.DeleteItem(itemData);
                        uiInventory.DeleteItemRT(rectTransform);
                        Destroy(gameObject);
                    }
                    else
                    {
                        Debug.Log("El cofre está lleno. Límite máximo de 5 objetos.");
                    }

                    droppedInValidArea = true; 
                    break;
                }
                
                // ==========================================
                // CASO 3: Soltamos en slots de EQUIPAMIENTO
                // ==========================================
                if (itemData.equipable)
                {
                    if (result.gameObject.CompareTag("EquipmentUI"))
                    {
                        if (itemData.tipoEquipo == result.gameObject.GetComponent<EquipmentSlot>().tipoDeSlot)
                        {
                            equipmenSlot = result.gameObject.GetComponent<EquipmentSlot>();
                            typeSlot = 0;
                            equipmenSlot.Equip(itemData.tipoEquipo, itemData, rectTransform);
                            startDrag = "Equipment";
                            droppedInValidArea = true;
                            return;
                        }
                    }
                    else if (result.gameObject.CompareTag("ReinforcementUI"))
                    {
                        reinfrocementSlot = result.gameObject.GetComponent<ReinfrocementSlot>();
                        typeSlot = 1;
                        reinfrocementSlot.SetReinforceItem(itemData, rectTransform);
                        startDrag = "Equipment";
                        droppedInValidArea = true;
                        return;
                    }
                    else if (result.gameObject.CompareTag("FragmentUI"))
                    {
                        fragmentSlot = result.gameObject.GetComponent<FragmentSlot>();
                        typeSlot = 2;
                        fragmentSlot.SetFragmentItem(itemData, rectTransform);
                        startDrag = "Equipment";
                        droppedInValidArea = true;
                        return;
                    }
                }
            } // <-- AQUÍ SE CIERRA EL FOREACH CORRECTAMENTE

            // Si después de revisar todo no cayó en un lugar válido, lo tiramos al suelo
            if (!droppedInValidArea)
            {
                leave(false);
            }
        }
        else
        {
            // Cayó dentro de su mismo panel original
            originalParent = transform.parent;
            transform.SetParent(originalParent);
            if (startDrag == "Equipment")
            {
                if (typeSlot == 0)
                {
                    Debug.Log("Normal equipmenSlot");
                    equipmenSlot.UnEquip(itemData.tipoEquipo, rectTransform, null, false);
                }
                else if (typeSlot == 1)
                {
                    Debug.Log("Normal reinfrocementSlot");
                    reinfrocementSlot.UnEquip(rectTransform, null, false);
                    reinfrocementSlot.placeHolder.SetActive(false);
                }
                else
                {
                    fragmentSlot.UnEquip(rectTransform, null, false);
                    fragmentSlot.placeHolder.SetActive(false);
                }

            }
        }
        isDragged = false;
    }
    public void SetNormalMeasures()
    {
        Debug.Log("Volver a poner medidas originales");
        rectTransform.sizeDelta = new Vector2(itemData.sizeX * 83.3f + 20f, itemData.sizeY * 83.3f + 20f);
        rectTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(400, 280);
        rectTransform.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);
        rectTransform.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(itemData.sizeX * 83.3f, itemData.sizeY * 83.3f);
        startDrag = "Normal";
    }
    private int FindPrefabIndex(GameObject prefab)
    {
        // Usamos .Prefabs aquí también
        var list = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Prefab == prefab) return i;
        }
        return -1;
    }
    private uint GetPrefabHash(GameObject prefab)
    {
        if (prefab == null) return 0;

        // Buscamos en la lista de prefabs del NetworkManager el hash correspondiente
        foreach (var networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (networkPrefab.Prefab == prefab)
            {
                return networkPrefab.SourcePrefabGlobalObjectIdHash;
            }
        }
        return 0;
    }
    public void leave(bool died)
    {
        Debug.Log("Item Dropped");
        itemData.prefab.SetActive(true);
        //Player = GameObject.FindWithTag("Player");
        inventory = Player.GetComponent<Inventory>();
        Vector3 dropPos = new Vector3();
        if (!died)
        {
            dropPos = Player.transform.position + Player.transform.forward * 0.35f + new Vector3(0f, 0.2f, 0f);
        }
        else if (died)
        {
            dropPos = Player.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.4f, Random.Range(-0.5f, 0.5f));
        }
        uint hash = GetPrefabHash(itemData.prefab);

        if (hash != 0)
        {
            // Obtenemos el jugador local y enviamos solo el Hash y la posición
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
            localPlayer.RequestDropItemServerRpc(hash, dropPos, Quaternion.identity, itemData.Daño, itemData.durabilidad, itemData.currentDurability, itemData.Armadura, itemData.quantity, itemData.rarezaEquipo);
            //public void RequestDropItemServerRpc(uint prefabHash, Vector3 position, Quaternion rotation, float Damage, float Durability , float CurrentDurability , float Armor , float Cuantity , string Quality)
        }
        if (startDrag == "Equipment")
        {
            if (typeSlot == 0)
            {
                equipmenSlot.UnEquip(itemData.tipoEquipo, rectTransform, null, false);
            }
            else if (typeSlot == 1)
            {
                reinfrocementSlot.UnEquip(rectTransform, null, false);
            }
            else if (typeSlot == 2)
            {
                fragmentSlot.UnEquip(rectTransform, null, false);
                fragmentSlot.currentFragmentItem = null;
            }
        }
        inventory.capacity -= itemData.sizeX * itemData.sizeY;
        canvas.capacity1.text = inventory.capacity.ToString();
        uiInventory = GameObject.FindWithTag("UI").GetComponent<InventoryUI>();
        Debug.Log(quantity);
        inventory.DeleteItem(itemData);
        uiInventory.DeleteItemRT(rectTransform);
        Destroy(gameObject);

    }
    [ServerRpc]
    public void NotifyItemDestroyedServerRpc()
    {
        var netItem = itemDrop.GetComponent<NetworkObject>();
        if (netItem != null)
        {
            netItem.Spawn();
        }
    }
    public void UpdateQuantity()
    {
        quantity = itemData.quantity;
        quantityText.text = quantity.ToString();
    }
    public void ChangeIndicators()
    {
        if (itemData.tipoEquipo == TipoEquipo.Weapon)
        {
            other.text = "Damage: " + itemData.Daño.ToString();
        }
        else
        {
            other.text = "Armor: " + itemData.Armadura.ToString();
        }
        quality.text = "Quality: " + itemData.rarezaEquipo.ToString();
        SetupDurability();
    }
    public void SetupDurability()
    {
        durability1.maxValue = itemData.durabilidad;
        durability2.maxValue = itemData.durabilidad;
        ChangeDurabilty();
    }
    public void ChangeDurabilty()
    {
        durability1.value = itemData.currentDurability;
        durability2.value = itemData.currentDurability;
        durabilityText.text = itemData.currentDurability.ToString();
    }
    public void Destroy()
    {
        if (itemData.currentDurability <= 0)
        {
            inventory.DeleteItem(itemData);
            uiInventory.DeleteItemRT(rectTransform);
            if (equipmenSlot != null)
            {
                equipmenSlot.UnEquip(itemData.tipoEquipo, rectTransform, null, false);
            }
            Player.GetComponent<Player>().StartCoroutine(Player.GetComponent<Player>().BreakItem(this.gameObject));
        }
    }
}