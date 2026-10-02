using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq; // Necesario para ordenar las listas

public class ReinfrocementSlot : MonoBehaviour
{// Configura esto en el Inspector para cada slot (Arma, Casco, etc.)
    public RectTransform ThisSlot;
    public Texture OriginalImage;
    public RectTransform equipeArmor = null;
    public ItemData itemData;
    public Canvas_Controller canvas;
    public ItemData currentReinforceItem;
    public float[] rarityMult = { 1.0f, 1.2f, 1.4f, 1.6f, 1.8f, 2.0f };
    public ItemDataBase itemsToReinforce;
    public ItemDataBase materialsForReinforce;
    public Dictionary<int, List<Dictionary<ItemData, int>>> itemsCost = new Dictionary<int, List<Dictionary<ItemData, int>>>();
    public Image mat1, mat2;
    public TextMeshProUGUI qty1, qty2, qualityText, newQualityText, statsText, newStatsText, durabilityText, newDurabilityText, title;
    public float stats, newStats, durability, newDurability;
    public int qty_1, qty_2;
    public string quality, newQuality;
    public ItemData newItem;
    public ItemIconGenerator generator;
    public GameObject placeHolder;
    private ItemData itemNeeded1, itemNeeded2;
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
            int num = Random.Range(1, 4);
            recetaNivel1.Add(materialAleatorio1, num);
            recetaNivel1.Add(materialAleatorio2, 5 - num);

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
    public ItemData ChangeStats(ItemData item)
    {
        switch (item.rarezaEquipo)
        {
            case Rareza.Common:
                item.Daño /= rarityMult[0];
                item.Armadura /= rarityMult[0];
                item.durabilidad /= rarityMult[0];
                item.Daño *= rarityMult[1];
                item.Armadura *= rarityMult[1];
                item.durabilidad *= rarityMult[1];
                item.rarezaEquipo = Rareza.Rare;
                break;
            case Rareza.Rare:
                item.Daño /= rarityMult[1];
                item.Armadura /= rarityMult[1];
                item.durabilidad /= rarityMult[1];
                item.Daño *= rarityMult[2];
                item.Armadura *= rarityMult[2];
                item.durabilidad *= rarityMult[2];
                item.rarezaEquipo = Rareza.Super_rare;
                break;
            case Rareza.Super_rare:
                item.Daño /= rarityMult[2];
                item.Armadura /= rarityMult[2];
                item.durabilidad /= rarityMult[2];
                item.Daño *= rarityMult[3];
                item.Armadura *= rarityMult[3];
                item.durabilidad *= rarityMult[3];
                item.rarezaEquipo = Rareza.Epic;
                break;
            case Rareza.Epic:
                item.Daño /= rarityMult[3];
                item.Armadura /= rarityMult[3];
                item.durabilidad /= rarityMult[3];
                item.Daño *= rarityMult[4];
                item.Armadura *= rarityMult[4];
                item.durabilidad *= rarityMult[4];
                item.rarezaEquipo = Rareza.Legendary;
                break;
            case Rareza.Legendary:
                item.Daño /= rarityMult[4];
                item.Armadura /= rarityMult[4];
                item.durabilidad /= rarityMult[4];
                item.Daño *= rarityMult[5];
                item.Armadura *= rarityMult[5];
                item.durabilidad *= rarityMult[5];
                item.rarezaEquipo = Rareza.Mhytic;
                break;
            case Rareza.Mhytic:
                item.Daño++;
                item.Armadura += 0.5f;
                item.durabilidad += 10;
                break;
        }
        Debug.Log("Nuevo Daño: " + item.Daño);
        Debug.Log("Nueva Armadura: " + item.Armadura);
        item.currentDurability = item.durabilidad;
        return item;
    }
    public void SetStats(ItemData item)
    {
        newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.id = item.id;
        newItem.tipoEquipo = item.tipoEquipo;
        newItem.rarezaEquipo = item.rarezaEquipo;
        newItem.Daño = item.Daño;
        newItem.Armadura = item.Armadura;
        newItem.durabilidad = item.durabilidad;

        if (newItem.tipoEquipo == TipoEquipo.Weapon)
        {
            stats = newItem.Daño;
            durability = newItem.durabilidad;
        }
        else
        {
            stats = newItem.Armadura;
            durability = newItem.durabilidad;
        }
        quality = newItem.rarezaEquipo.ToString();
        newItem = ChangeStats(newItem);
        if (newItem.tipoEquipo == TipoEquipo.Weapon)
        {
            newStats = newItem.Daño;
            newDurability = newItem.durabilidad;
        }
        else
        {
            newStats = newItem.Armadura;
            newDurability = newItem.durabilidad;
        }
        newQuality = newItem.rarezaEquipo.ToString();
        statsText.text = stats.ToString("F2");
        newStatsText.text = newStats.ToString("F2");
        durabilityText.text = durability.ToString("F0");
        newDurabilityText.text = newDurability.ToString("F0");
        qualityText.text = quality;
        newQualityText.text = newQuality;
    }
    public void Reinforce()
    {
        // 2. Verificar materiales totales
        int count1 = playerInventory.items.Where(i => i.id == itemNeeded1.id).Sum(i => i.quantity);
        int count2 = playerInventory.items.Where(i => i.id == itemNeeded2.id).Sum(i => i.quantity);

        if (count1 < qty_1 || count2 < qty_2)
        {
            canvas.ShowAdvise("Not enough materials to reinforce the item", Color.yellow);
            successSource.Stop();
            failSource.Play();
            return;
        }
        RemoveItems(playerInventory, itemNeeded1.id, qty_1);
        RemoveItems(playerInventory, itemNeeded2.id, qty_2);
        currentReinforceItem = ChangeStats(currentReinforceItem);
        canvas.ShowAdvise("Item Reinforced successfully", Color.green);
        failSource.Stop();
        successSource.Play();
        ManagePlaceHolder(currentReinforceItem);
    }
    public void ManagePlaceHolder(ItemData item)
    {
        qty_1 = itemsCost[item.id][0].Values.ElementAt(0);
        qty_2 = itemsCost[item.id][0].Values.ElementAt(1);
        SetupNewItem(currentReinforceItem);
        itemNeeded1 = itemsCost[item.id][0].Keys.ElementAt(0);
        itemNeeded2 = itemsCost[item.id][0].Keys.ElementAt(1);
        mat1.sprite = generator.GenerateIcon(itemsCost[item.id][0].Keys.ElementAt(0).prefab, itemsCost[item.id][0].Keys.ElementAt(0));
        qty1.text = qty_1.ToString();
        mat2.sprite = generator.GenerateIcon(itemsCost[item.id][0].Keys.ElementAt(1).prefab, itemsCost[item.id][0].Keys.ElementAt(1));
        qty2.text = qty_2.ToString();
        SetStats(currentReinforceItem);
        if (item.tipoEquipo == TipoEquipo.Weapon)
        {
            title.text = "Damage";
        }
        else
        {
            title.text = "Armor";
        }
        placeHolder.SetActive(true);
        OriginalItem.ChangeIndicators();
    }
    public void SetReinforceItem(ItemData item, RectTransform transform)
    {
        Debug.Log("SetReinforceItem");
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
                Debug.Log("Desequipar por solapamiento");
            }
            transform.position = ThisSlot.position;
            transform.sizeDelta = ThisSlot.sizeDelta / 2.2f;
            transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = ThisSlot.sizeDelta / 2.2f;
            equipeArmor = transform;
            OriginalItem.equiped = true;
            currentReinforceItem = item;
            ManagePlaceHolder(currentReinforceItem);
        }
    }
    public void UnEquip(RectTransform transformOriginal, RectTransform transformNuevo, bool bug)
    {
        Debug.Log("Desequipar");
        //Bug = si se desactiva de forma natural, si es true es que se han solapadao armaduras
        DraggableItemUI OriginalItem = transformOriginal.GetComponent<DraggableItemUI>();
        if (bug == false)
        {
            equipeArmor = null;
        }
        OriginalItem.equiped = false;
        if (bug == true)
        {
            DraggableItemUI NewItem = transformNuevo.GetComponent<DraggableItemUI>();
        }
        currentReinforceItem = null;
        OriginalItem.equiped = false;
        Debug.Log("Placehorlders remove");
        OriginalItem.SetNormalMeasures();
        placeHolder.SetActive(false);
    }

    private void RemoveItems(Inventory inv, int id, int amountToRemove)
    {
        // 1. Obtener stacks del material ordenados por cantidad (1, 2, 3...)
        var sortedStacks = inv.items
            .Select((item, index) => new { item, index })
            .Where(x => x.item.id == id)
            .OrderBy(x => x.item.quantity)
            .ToList();

        List<int> indicesToRemove = new List<int>();

        foreach (var entry in sortedStacks)
        {
            if (amountToRemove <= 0) break;
            if (entry.item.quantity <= amountToRemove)
            {
                amountToRemove -= entry.item.quantity;
                indicesToRemove.Add(entry.index);
            }
            else
            {
                entry.item.quantity -= amountToRemove;
                ui.existingItems[entry.index].GetComponent<DraggableItemUI>().UpdateQuantity();
                amountToRemove = 0;
            }
        }
        // 2. Borrado físico (de mayor índice a menor para no corromper la lista)
        foreach (int i in indicesToRemove.OrderByDescending(x => x))
        {
            inv.capacity -= inv.items[i].sizeX * inv.items[i].sizeY;
            canvas.capacity1.text = inv.capacity.ToString();

            Destroy(ui.existingItems[i].gameObject);
            ui.existingItems.RemoveAt(i);
            inv.items.RemoveAt(i);
        }
    }
}
