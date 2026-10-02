using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq; // Necesario para ordenar las listas (LINQ)

public class ChangeMaterials : MonoBehaviour
{
    [Header("Setup de Materiales")]
    public MaterialSetup materialSetup1; // El que entregas
    public MaterialSetup materialSetup2; // El que recibes

    [Header("Referencias UI")]
    public TMP_InputField quantiity;
    public int quantityNum;
    public Canvas_Controller canvas;

    [Header("Sistemas de Inventario")]
    public GameObject player;
    public Inventory playerInventory;
    public InventoryUI ui;
    public ItemIconGenerator generator;
    public AudioSource successSource, failSource;
    [HideInInspector] public bool exit = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>();
        }
        generator = FindObjectOfType<ItemIconGenerator>();
        quantityNum = 1;
    }

    // Se llama cuando el usuario cambia el texto en el InputField
    public void ChangeQuantity()
    {
        if (int.TryParse(quantiity.text, out int valor))
        {
            if (valor < 1) valor = 1;
            quantityNum = valor;
            quantiity.text = valor.ToString();
        }
        else
        {
            quantiity.text = "1";
            quantityNum = 1;
        }
    }

    // Método principal del botón de intercambio
    public void Exchange()
    {
        exit = true;
        int numToExchange = quantityNum;

        // 1. Verificar si hay materiales suficientes (Sumamos todas las cantidades de ese ID)
        int currentMaterialCount = playerInventory.items
            .Where(i => i.id == materialSetup1.currentItem.id)
            .Sum(i => i.quantity);

        if (currentMaterialCount < numToExchange)
        {
            canvas.ShowAdvise("No tienes suficientes materiales.", Color.red);
            successSource.Stop();
            failSource.Play();
            return;
        }

        // 2. SIMULACIÓN: ¿Cabrán los nuevos items?
        if (!HasInventorySpace(numToExchange))
        {
            canvas.ShowAdvise("No hay espacio suficiente en el inventario.", Color.yellow);
            successSource.Stop();
            failSource.Play();
            return;
        }

        // 3. PROCESO REAL: Primero eliminamos (los más pequeños primero)
        RemoveItems(playerInventory, materialSetup1.currentItem.id, numToExchange);

        // 4. PROCESO REAL: Añadimos (rellenando los más llenos primero)
        AddItem(numToExchange);

        canvas.ShowAdvise("¡Intercambio completado!", Color.green);
        failSource.Stop();
        successSource.Play();
    }

    private bool HasInventorySpace(int totalQuantity)
    {
        int maxStack = 10;
        int remainingToAdd = totalQuantity;

        // A. Ver cuánto cabe en stacks existentes del material que vamos a recibir
        foreach (var item in playerInventory.items.Where(i => i.id == materialSetup2.currentItem.id))
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
            .Where(i => i.id == materialSetup1.currentItem.id)
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

    private void AddItem(int totalQuantity)
    {
        int remainingToAdd = totalQuantity;
        int maxStack = 10;

        // PASO 1: Llenar stacks existentes PRIORIZANDO los que están casi llenos (9, 8, 7...)
        var stacksToFill = playerInventory.items
            .Select((item, index) => new { item, index }) // Guardamos el índice original para la UI
            .Where(x => x.item.id == materialSetup2.currentItem.id && x.item.quantity < maxStack)
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
            CopyData(materialSetup2.currentItem, newItem);
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
                exit = false;
                break;
            }
        }
    }

    private void RemoveItems(Inventory inv, int id, int amountToRemove)
    {
        // 1. Identificar stacks y sus índices, ordenados por cantidad (1, 2, 3...)
        var sortedMaterials = inv.items
            .Select((item, index) => new { item, index })
            .Where(x => x.item.id == id)
            .OrderBy(x => x.item.quantity) // <--- Los más pequeños primero
            .ToList();

        List<int> indicesToRemove = new List<int>();

        foreach (var entry in sortedMaterials)
        {
            if (amountToRemove <= 0) break;

            if (entry.item.quantity <= amountToRemove)
            {
                amountToRemove -= entry.item.quantity;
                indicesToRemove.Add(entry.index); // Marcamos para borrar después
            }
            else
            {
                entry.item.quantity -= amountToRemove;
                ui.existingItems[entry.index].GetComponent<DraggableItemUI>().UpdateQuantity();
                amountToRemove = 0;
            }
        }

        // 2. Borrar físicamente (IMPORTANTE: Orden descendente para no romper los índices)
        foreach (int i in indicesToRemove.OrderByDescending(x => x))
        {
            // Actualizar capacidad antes de borrar
            inv.capacity -= inv.items[i].sizeX * inv.items[i].sizeY;
            if (canvas.capacity1 != null) canvas.capacity1.text = inv.capacity.ToString();

            Destroy(ui.existingItems[i].gameObject);
            ui.existingItems.RemoveAt(i);
            inv.items.RemoveAt(i);
        }
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