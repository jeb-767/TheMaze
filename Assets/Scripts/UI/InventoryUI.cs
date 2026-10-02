using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    private GameObject[,] slots;
    public Transform itemsParent;
    public Transform itemsChestParent;
    public GameObject InventoryTransform;  // Contenedor principal de ítems
    public GameObject Image_BackGroundPrefab;
    public Inventory inventory;
    public RectTransform currentDraggedItem;

    // Lista para almacenar todos los RectTransform ya colocados
    public List<RectTransform> existingItems = new List<RectTransform>();

    private const int MAX_ATTEMPTS = 100;
    public int index = 0;
    public void DeleteItemRT(RectTransform rt)
    {
        existingItems.Remove(rt);
    }
    public void AddItemRT(RectTransform rt)
    {
        existingItems.Add(rt);
    }
    public void InitGrid(int width, int height)
    {
        // (si no lo usas de momento, puedes dejarlo vacío)
    }
    public void Start()
    {
        //inventory = GameObject.FindWithTag("Player").GetComponent<Inventory>();
    }
    public void PlaceItem(ItemData item)
    {
        GameObject Image_BackGround = Instantiate(Image_BackGroundPrefab, itemsParent);
        DraggableItemUI draggable = Image_BackGround.GetComponent<DraggableItemUI>();
        draggable.itemData = item; // 'item' es tu ItemData
        Image img = Image_BackGround.GetComponent<RectTransform>().GetChild(2).GetComponent<Image>(); //Child (2) = imagen item
        draggable.Player = inventory.canvas.player.gameObject;
        img.sprite = item.icon;
        draggable.quantity = item.quantity;
        draggable.uiInventory = this;
        draggable.inventory = inventory;
        draggable.Player = inventory.canvas.player.gameObject;
        RectTransform rt_bg = Image_BackGround.GetComponent<RectTransform>();
        // Calcula la posición de la esquina superior izquierda
        // Calcula la posición del objeto
        rt_bg.anchoredPosition = new Vector2(Random.Range(95, 1250f), Random.Range(-100f, -730f));
        rt_bg.sizeDelta = new Vector2(item.sizeX * 83.3f + 20f, item.sizeY * 83.3f + 20f);
        rt_bg.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(item.sizeX * 83.3f, item.sizeY * 83.3f);
        if (!InventoryTransform.activeInHierarchy)
        {
            InventoryTransform.SetActive(true);
            InventoryTransform.SetActive(false);
        }

        // Intentar colocar sin solapamiento
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            Vector2 randomPos = new Vector2(Random.Range(100f, 1250f), Random.Range(-100f, -730f));
            rt_bg.anchoredPosition = randomPos;

            if (!IsOverlappingAny(rt_bg))
            {
                // No se solapa con nadie -> posición válida
                existingItems.Add(rt_bg);
                if (!InventoryTransform.activeInHierarchy)
                {
                    InventoryTransform.SetActive(true);
                    InventoryTransform.SetActive(false);
                }
                inventory.AddItem(item);
                Debug.Log("No Existe , cantidad:" + item.quantity);
                return;
            }
        }
    }
    public void PlaceItemChest(ItemData item)
    {
        GameObject Image_BackGround = Instantiate(Image_BackGroundPrefab, itemsChestParent);
        DraggableItemUI draggable = Image_BackGround.GetComponent<DraggableItemUI>();
        draggable.itemData = item; // 'item' es tu ItemData
        Image img = Image_BackGround.GetComponent<RectTransform>().GetChild(2).GetComponent<Image>(); //Child (2) = imagen item
        draggable.Player = inventory.canvas.player.gameObject;
        img.sprite = item.icon;
        draggable.quantity = item.quantity;
        draggable.uiInventory = this;
        draggable.inventory = inventory;
        draggable.Player = inventory.canvas.player.gameObject;
        RectTransform rt_bg = Image_BackGround.GetComponent<RectTransform>();
        // Calcula la posición de la esquina superior izquierda
        // Calcula la posición del objeto
        rt_bg.anchoredPosition = new Vector2(Random.Range(95f, 552f), Random.Range(-168f, -971f));
        rt_bg.sizeDelta = new Vector2(item.sizeX * 83.3f + 20f, item.sizeY * 83.3f + 20f);
        rt_bg.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(item.sizeX * 83.3f, item.sizeY * 83.3f);
        if (!InventoryTransform.activeInHierarchy)
        {
            InventoryTransform.SetActive(true);
            itemsChestParent.gameObject.SetActive(true);
            itemsChestParent.gameObject.SetActive(false);
            InventoryTransform.SetActive(false);
        }

        // Intentar colocar sin solapamiento
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            Vector2 randomPos = new Vector2(Random.Range(95f, 552f), Random.Range(-168f, -822f));
            rt_bg.anchoredPosition = randomPos;

            if (!IsOverlappingAny(rt_bg))
            {
                // No se solapa con nadie -> posición válida

                if (!InventoryTransform.activeInHierarchy)
                {
                    InventoryTransform.SetActive(true);
                    itemsChestParent.gameObject.SetActive(true);
                    itemsChestParent.gameObject.SetActive(false);
                    InventoryTransform.SetActive(false);
                }
                Debug.Log("No Existe , cantidad:" + item.quantity);
                return;
            }
        }
    }
    public void UpdateQuantityText(ItemData item)
    {
        Debug.Log("Existe , cantidad:" + item.quantity + item.name);
        inventory.items[index].quantity += item.quantity;
        existingItems[index].GetComponent<DraggableItemUI>().quantity = inventory.items[index].quantity;
        existingItems[index].GetComponent<DraggableItemUI>().UpdateQuantity();
    }
    public bool CheckItemQuantity(ItemData item)
    {
        foreach (ItemData rt in inventory.items)
        {
            if (rt.id == item.id && (rt.quantity + item.quantity) <= 10)
            {
                index = inventory.items.IndexOf(rt);
                return true;
            }
        }
        return false;
    }
    // Comprueba si un rectTransform se solapa con alguno ya colocado
    private bool IsOverlappingAny(RectTransform current)
    {
        foreach (RectTransform other in existingItems)
        {
            if (IsOverlapping(current, other))
                return true;
        }
        return false;
    }

    // Comprueba si dos rectTransforms se solapan
    private bool IsOverlapping(RectTransform a, RectTransform b)
    {
        Rect rectA = GetScreenRect(a);
        Rect rectB = GetScreenRect(b);
        return rectA.Overlaps(rectB, true);
    }

    // Convierte un RectTransform a su área en pantalla
    private Rect GetScreenRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4]; 
        rectTransform.GetWorldCorners(corners);
        float xMin = corners[0].x;
        float yMin = corners[0].y;
        float width = corners[2].x - xMin;
        float height = corners[2].y - yMin;
        return new Rect(xMin, yMin, width, height);
    }
}
