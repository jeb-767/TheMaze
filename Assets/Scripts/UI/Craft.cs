using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq; // Necesario para ordenar las listas

public class Craft : MonoBehaviour
{
    public GameObject player;
    public InventoryUI ui;
    public Canvas_Controller canvas;
    public ItemIconGenerator generator;
    public CraftSetup craftSetup;
    public Inventory playerInventory;
    public AudioSource successSource, failSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerInventory = player.GetComponent<Inventory>();
        craftSetup = GetComponent<CraftSetup>();
        generator = FindObjectOfType<ItemIconGenerator>();
    }

    public void CraftItem(GameObject item)
    {
        // 1. Seleccionar el item resultante (necesario para saber el espacio que ocupará)
        ItemData itemCrafteado = ScriptableObject.CreateInstance<ItemData>();
        ItemData randomBase = craftSetup.itemToCraft.objects[Random.Range(0, craftSetup.itemToCraft.objects.Count)];
        CopyData(randomBase, itemCrafteado);

        // 2. Verificar materiales totales
        int count1 = playerInventory.items.Where(i => i.id == craftSetup.itemForCraft1.id).Sum(i => i.quantity);
        int count2 = playerInventory.items.Where(i => i.id == craftSetup.itemForCraft2.id).Sum(i => i.quantity);

        if (count1 < craftSetup.quantityNeeded || count2 < craftSetup.quantityNeeded2)
        {
            canvas.ShowAdvise("Not enough materials to craft the item", Color.yellow);
            failSource.Play();
            successSource.Stop();
            return;
        }

        // 3. SIMULACIÓN DE ESPACIO (Considerando eliminación de stacks pequeños)
        if (!CanFitCraftedItem(itemCrafteado))
        {
            canvas.ShowAdvise("Not enough space in inventory", Color.yellow);
            failSource.Play();
            successSource.Stop();
            return;
        }

        // 4. EJECUCIÓN: Eliminar materiales (menor cantidad primero)
        RemoveItems(playerInventory, craftSetup.itemForCraft1.id, craftSetup.quantityNeeded);
        RemoveItems(playerInventory, craftSetup.itemForCraft2.id, craftSetup.quantityNeeded2);

        // Configurar estadísticas y añadir
        SetupItemStats(itemCrafteado);
        itemCrafteado.icon = generator.GenerateIcon(itemCrafteado.prefab, itemCrafteado);

        playerInventory.FillSpace(itemCrafteado);
        ui.PlaceItem(itemCrafteado);

        canvas.ShowAdvise("Item crafted successfully", Color.green);
        successSource.Play();
        failSource.Stop();
    }

    private bool CanFitCraftedItem(ItemData resultItem)
    {
        int areaRequired = resultItem.sizeX * resultItem.sizeY;
        int areaToBeFreed = 0;

        areaToBeFreed += CalculateFreedArea(craftSetup.itemForCraft1.id, craftSetup.quantityNeeded);
        areaToBeFreed += CalculateFreedArea(craftSetup.itemForCraft2.id, craftSetup.quantityNeeded2);

        int maxCapacity = 50;
        int currentFreeArea = maxCapacity - playerInventory.capacity;

        return (currentFreeArea + areaToBeFreed) >= areaRequired;
    }

    private int CalculateFreedArea(int id, int amountToRemove)
    {
        int freed = 0;
        int tempAmount = amountToRemove;

        // Simulamos eliminando los más pequeños primero para ver cuántos huecos reales se vacían
        var sortedForSim = playerInventory.items
            .Where(i => i.id == id)
            .OrderBy(i => i.quantity);

        foreach (var item in sortedForSim)
        {
            if (tempAmount <= 0) break;
            if (item.quantity <= tempAmount)
            {
                tempAmount -= item.quantity;
                freed += item.sizeX * item.sizeY;
            }
            else tempAmount = 0;
        }
        return freed;
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

    private void SetupItemStats(ItemData item)
    {
        // Tu lógica de stats...
        if (item.tipoEquipo == TipoEquipo.Weapon)
        {
            item.Daño = Mathf.Round(((float)(Random.Range(6, 12))) * 100f) / 100f;
            item.Armadura = 0f;
        }
        else
        {
            item.Armadura = Mathf.Round(((float)(Random.Range(2, 7) / 10f)) * 100f) / 100f;
            item.Daño = 0f;
        }

        float multiplier = 1.0f + ((craftSetup.quantityAvailable / 5 - 1) * 0.2f);
        item.Daño *= multiplier;
        item.Armadura *= multiplier;

        Rareza[] rarezas = { Rareza.Common, Rareza.Rare, Rareza.Super_rare, Rareza.Epic, Rareza.Legendary, Rareza.Mhytic };
        int index = Mathf.Clamp((craftSetup.quantityAvailable / 5) - 1, 0, rarezas.Length - 1);
        item.rarezaEquipo = rarezas[index];
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
    }
}