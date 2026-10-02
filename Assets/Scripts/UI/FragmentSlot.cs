using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq; // Necesario para ordenar las listas

public class FragmentSlot : MonoBehaviour
{// Configura esto en el Inspector para cada slot (Arma, Casco, etc.)
    public RectTransform ThisSlot;
    public Texture OriginalImage;
    public RectTransform equipeArmor = null;
    public ItemData itemData;
    public Canvas_Controller canvas;
    public ItemData currentFragmentItem;
    public float[] rarityMult = { 1.0f, 1.2f, 1.4f, 1.6f, 1.8f, 2.0f };
    public ItemDataBase itemsToReinforce;
    public ItemDataBase materialsForReinforce;
    public Dictionary<int, List<Dictionary<ItemData, int>>> itemsCost = new Dictionary<int, List<Dictionary<ItemData, int>>>();
    public Image mat1, mat2;
    public int qty_1, qty_2;
    public TextMeshProUGUI qty1, qty2;
    public ItemIconGenerator generator;
    public GameObject placeHolder;
    private ItemData itemGiven1, itemGiven2;
    public Inventory playerInventory;
    public InventoryUI ui;
    public DraggableItemUI OriginalItem;
    public RectTransform actualItem;
    public AudioSource successSource, failSource;
    void Start()
    {
        generator = FindObjectOfType<ItemIconGenerator>();
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        placeHolder.SetActive(false);
        RawImage rawImage = ThisSlot.GetComponent<RawImage>();
        OriginalImage = rawImage.texture;
        foreach (ItemData item in itemsToReinforce.objects)
        {
            // 1. Inicializamos la lista de recetas para este item específico
            itemsCost[item.id] = new List<Dictionary<ItemData, int>>();

            // 2. Creamos un "diccionario de materiales" para el Nivel 1 de mejora
            Dictionary<ItemData, int> recetaNivel1 = new Dictionary<ItemData, int>();

            // --- LÓGICA AUTOMÁTICA PARA ELEGIR MATERIALES ---

            // Elegimos 2 materiales distintos al azar de tu lista global de materiales
            int index1 = Random.Range(0, materialsForReinforce.objects.Count);
            int index2 = Random.Range(0, materialsForReinforce.objects.Count);

            // Evitamos que el segundo material sea igual al primero
            while (index2 == index1)
            {
                index2 = Random.Range(0, materialsForReinforce.objects.Count);
            }

            ItemData materialAleatorio1 = materialsForReinforce.objects[index1];
            ItemData materialAleatorio2 = materialsForReinforce.objects[index2];
            // 3. Asignamos cantidades aleatorias (ejemplo: entre 1 y 5)
            int num = Random.Range(1, 2);
            recetaNivel1.Add(materialAleatorio1, num);
            recetaNivel1.Add(materialAleatorio2, 3 - num);

            // 4. Guardamos la receta en la lista del item
            itemsCost[item.id].Add(recetaNivel1);

        }
    }
    public void SetupNewItem(ItemData item)
    {
        switch (item.rarezaEquipo)
        {
            case Rareza.Common:
                qty_1 *= 1;
                qty_2 *= 1;
                break;
            case Rareza.Rare:
                qty_1 *= 2;
                qty_2 *= 2;
                break;
            case Rareza.Super_rare:
                qty_1 *= 3;
                qty_2 *= 3;
                break;
            case Rareza.Epic:
                qty_1 *= 4;
                qty_2 *= 4;
                break;
            case Rareza.Legendary:
                qty_1 *= 5;
                qty_2 *= 5;
                break;
            case Rareza.Mhytic:
                qty_1 *= 6;
                qty_2 *= 6;
                break;
        }
    }
    public void Fragment()
    {
        // 2. Verificar materiales totales
        int count1 = playerInventory.items.Where(i => i.id == itemGiven1.id).Sum(i => i.quantity);
        int count2 = playerInventory.items.Where(i => i.id == itemGiven2.id).Sum(i => i.quantity);
        if (!HasInventorySpace(qty_1, itemGiven1) && !HasInventorySpace(qty_2, itemGiven2))
        {
            canvas.ShowAdvise("No hay espacio suficiente en el inventario.", Color.yellow);
            successSource.Stop();
            failSource.Play();
            return;
        }
        UnEquip(actualItem, null, false);
        RemoveItems(playerInventory, actualItem);
        AddItem(qty_1, itemGiven1);
        AddItem(qty_2, itemGiven2);
        canvas.ShowAdvise("Item Fragmented successfully", Color.green);
        failSource.Stop();
        successSource.Play();
        placeHolder.SetActive(false);
    }
    private bool HasInventorySpace(int totalQuantity, ItemData itemO)
    {
        int maxStack = 10;
        int remainingToAdd = totalQuantity;

        // A. Ver cuánto cabe en stacks existentes del material que vamos a recibir
        foreach (var item in playerInventory.items.Where(i => i.id == itemO.id))
        {
            int spaceInStack = maxStack - item.quantity;
            remainingToAdd -= Mathf.Min(remainingToAdd, spaceInStack);
        }

        if (remainingToAdd <= 0) return true; // Cabe todo sin crear nuevos slots

        // B. Calcular cuántos slots nuevos necesitamos para el resto
        int newSlotsNeeded = Mathf.CeilToInt((float)remainingToAdd / maxStack);

        // C. Calcular cuántos slots se vaciarán al entregar los materiales
        int slotsToBeFreed = 0;
        int tempAmountToRemove = totalQuantity;

        // Simulamos la eliminación (los más pequeños primero)
        var sortedForSim = playerInventory.items
            .Where(i => i.id == itemO.id)
            .OrderBy(i => i.quantity);

        foreach (var item in sortedForSim)
        {
            if (tempAmountToRemove <= 0) break;
            if (item.quantity <= tempAmountToRemove)
            {
                tempAmountToRemove -= item.quantity;
                slotsToBeFreed++; // El stack se borrará por completo
            }
            else tempAmountToRemove = 0;
        }

        // D. Verificamos contra la capacidad (asumiendo máximo 50)
        int currentlyFreeSlots = 50 - playerInventory.capacity;
        return (currentlyFreeSlots + slotsToBeFreed) >= newSlotsNeeded;
    }


    private void AddItem(int totalQuantity, ItemData item)
    {
        int remainingToAdd = totalQuantity;
        int maxStack = 10;

        // PASO 1: Llenar stacks existentes PRIORIZANDO los que están casi llenos (9, 8, 7...)
        var stacksToFill = playerInventory.items
            .Select((item, index) => new { item, index }) // Guardamos el índice original para la UI
            .Where(x => x.item.id == item.id && x.item.quantity < maxStack)
            .OrderByDescending(x => x.item.quantity) // <--- De más lleno a menos lleno
            .ToList();

        foreach (var entry in stacksToFill)
        {
            if (remainingToAdd <= 0) break;

            int spaceInStack = maxStack - entry.item.quantity;
            int amountToFill = Mathf.Min(remainingToAdd, spaceInStack);

            entry.item.quantity += amountToFill;
            remainingToAdd -= amountToFill;

            // Actualizar la cantidad visualmente en la UI
            ui.existingItems[entry.index].GetComponent<DraggableItemUI>().UpdateQuantity();
        }

        // PASO 2: Crear nuevos stacks si aún sobra cantidad
        while (remainingToAdd > 0)
        {
            int amountInNewStack = Mathf.Min(remainingToAdd, maxStack);
            ItemData newItem = ScriptableObject.CreateInstance<ItemData>();
            CopyData(item, newItem);
            newItem.quantity = amountInNewStack;

            if (playerInventory.CheckSpace(newItem))
            {
                if (newItem.icon == null && newItem.prefab != null)
                    newItem.icon = generator.GenerateIcon(newItem.prefab, newItem);

                playerInventory.FillSpace(newItem);
                ui.PlaceItem(newItem);
                remainingToAdd -= amountInNewStack;
            }
            else
            {
                break;
            }
        }
    }
    public void ManagePlaceHolder(ItemData item)
    {
        qty_1 = itemsCost[item.id][0].Values.ElementAt(0);
        qty_2 = itemsCost[item.id][0].Values.ElementAt(1);
        SetupNewItem(currentFragmentItem);
        itemGiven1 = itemsCost[item.id][0].Keys.ElementAt(0);
        itemGiven2 = itemsCost[item.id][0].Keys.ElementAt(1);
        mat1.sprite = generator.GenerateIcon(itemsCost[item.id][0].Keys.ElementAt(0).prefab, itemsCost[item.id][0].Keys.ElementAt(0));
        qty1.text = qty_1.ToString();
        mat2.sprite = generator.GenerateIcon(itemsCost[item.id][0].Keys.ElementAt(1).prefab, itemsCost[item.id][0].Keys.ElementAt(1));
        qty2.text = qty_2.ToString();
        placeHolder.SetActive(true);

    }
    public void SetFragmentItem(ItemData item, RectTransform transform)
    {
        OriginalItem = transform.GetComponent<DraggableItemUI>();
        actualItem = transform;
        if (item.equipable == true)
        {
            Debug.Log("Dentro If");

            if (equipeArmor != null)
            {
                /*Child(0) = description panel
                Child(1) = nombre panel
                Child(2) = imagen objeto*/
                equipeArmor.sizeDelta = new Vector2(item.sizeX * 83.3f + 20f, item.sizeY * 83.3f + 20f);
                equipeArmor.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(400, 280);
                equipeArmor.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(250, 50);
                equipeArmor.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(item.sizeX * 83.3f, item.sizeY * 83.3f);
                equipeArmor.anchoredPosition = new Vector2(Random.Range(100f, 1250f), Random.Range(-100f, -730f));
                UnEquip(transform, equipeArmor, true);
            }
            transform.position = ThisSlot.position;
            transform.sizeDelta = ThisSlot.sizeDelta / 2.2f;
            transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = ThisSlot.sizeDelta / 2.2f;
            equipeArmor = transform;
            OriginalItem.equiped = true;
            currentFragmentItem = item;
            ManagePlaceHolder(currentFragmentItem);
        }
    }
    public void UnEquip(RectTransform transformOriginal, RectTransform transformNuevo, bool bug)
    {
        //Bug = si se desactiva de forma natural, si es true es que se han solapadao armaduras
        OriginalItem = transformOriginal.GetComponent<DraggableItemUI>();
        if (bug == false)
        {
            equipeArmor = null;
        }
        OriginalItem.equiped = false;
        if (bug == true)
        {
            DraggableItemUI NewItem = transformNuevo.GetComponent<DraggableItemUI>();
        }
        OriginalItem.equiped = false;
        Debug.Log("Placehorlders remove");
        OriginalItem.SetNormalMeasures();
        placeHolder.SetActive(false);
    }

    private void RemoveItems(Inventory inv, RectTransform actualDraggableItem)
    {
        inv.capacity -= currentFragmentItem.sizeX * currentFragmentItem.sizeY;
        canvas.capacity1.text = inv.capacity.ToString();
        inv.DeleteItem(currentFragmentItem);
        ui.DeleteItemRT(actualDraggableItem);
        currentFragmentItem = null;
        Destroy(actualDraggableItem.gameObject);
    }
    private void CopyData(ItemData source, ItemData target)
    {
        target.id = source.id;
        target.itemName = source.itemName;
        target.description = source.description;
        target.prefab = source.prefab;
        target.sizeX = source.sizeX;
        target.sizeY = source.sizeY;
        target.tipoEquipo = source.tipoEquipo;
        target.equipable = source.equipable;
        // Agrega aquí más variables si ItemData tiene otras stats
    }
}
