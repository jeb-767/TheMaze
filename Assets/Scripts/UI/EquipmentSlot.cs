using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;
public class EquipmentSlot : MonoBehaviour
{
    public TipoEquipo tipoDeSlot; // Configura esto en el Inspector para cada slot (Arma, Casco, etc.)
    public RectTransform ThisSlot;
    public Texture OriginalImage;
    public Transform child;
    public Player_Armor_Setup setup;
    public RectTransform equipeArmor = null;
    public ItemData itemData;
    public Player Player;
    public GameObject canvas;

    public void Start()
    {
        //canvas = GameObject.FindWithTag("UI");
        //Player = GameObject.FindWithTag("Player").GetComponent<Player>();
        RawImage rawImage = ThisSlot.GetComponent<RawImage>();
        OriginalImage = rawImage.texture;
    }
    public void Equip(TipoEquipo slot, ItemData item, RectTransform transform)
    {
        // --- PARTE UI (Mantenida igual) ---
        DraggableItemUI OriginalItem = transform.GetComponent<DraggableItemUI>();

        if (equipeArmor != null)
        {
            // Lógica de intercambio de items (swap)
            equipeArmor.transform.SetParent(equipeArmor.GetComponent<DraggableItemUI>().inventoryPanel); // Desemparentar del slot
            equipeArmor.anchoredPosition = new Vector2(Random.Range(100f, 1250f), Random.Range(-100f, -730f));
            equipeArmor.sizeDelta = new Vector2(item.sizeX * 83.3f + 20f, item.sizeY * 83.3f + 20f);
            equipeArmor.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(item.sizeX * 83.3f, item.sizeY * 83.3f);
            UnEquip(tipoDeSlot, transform, equipeArmor, true);
        }
        else
        {
            Player.armor += OriginalItem.itemData.Armadura;
            if (OriginalItem.canvas.armorText)
                OriginalItem.canvas.armorText.text = Player.armor.ToString("F2");
        }

        // --- CORRECCIÓN CLAVE ---
        // 1. Añadimos a las listas locales para lógica interna (stats, drops, etc)
        if (slot == TipoEquipo.Pants || slot == TipoEquipo.Chest || slot == TipoEquipo.Helmet || /* etc */ slot == TipoEquipo.Belt)
        {
            Player.armorEquiped.Add(OriginalItem);
        }
        else if (slot == TipoEquipo.Weapon) {Player.swordEquiped = OriginalItem; Player.DañoAct = OriginalItem.itemData.Daño + RunManager.attack;}
        else if (slot == TipoEquipo.Tourch)
        {
            Player.torchEquiped = OriginalItem;
            Player.StartCoroutine("torchDurability");
        }

        // 2. LLAMADA A RED (Visuales 3D)
        // Ya no buscamos en setup.parts. Confiamos ciegamente en el Player.
        // Usamos el nuevo método que acepta el TipoEquipo.
        Player.RequestEquipItem(this.tipoDeSlot, item.id);

        // --- FIN CORRECCIÓN ---

        // Resto de lógica UI (Mover el icono al slot)
        transform.position = ThisSlot.position;
        transform.SetParent(ThisSlot); // Asegúrate de emparentarlo
        transform.sizeDelta = ThisSlot.sizeDelta;
        transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = ThisSlot.sizeDelta;
        equipeArmor = transform;
        OriginalItem.equiped = true;

        // Actualizar capacidad inventario
        OriginalItem.inventory.capacity -= OriginalItem.itemData.sizeX * OriginalItem.itemData.sizeY;
        if (OriginalItem.canvas.capacity1) OriginalItem.canvas.capacity1.text = OriginalItem.inventory.capacity.ToString();

        canvas.GetComponent<InventoryUI>().DeleteItemRT(transform);
        Player.GetComponent<Inventory>().DeleteItem(item);
    }

    public void UnEquip(TipoEquipo slot, RectTransform transformOriginal, RectTransform transformNuevo, bool bug)
    {
        DraggableItemUI OriginalItem = transformOriginal.GetComponent<DraggableItemUI>();

        // --- CORRECCIÓN ---
        // En lugar de iterar setup.parts y apagar cosas, mandamos señal de desequipar (-1)
        // Solo si NO es un "bug" (intercambio), porque si es intercambio, el Equip() siguiente se encargará.
        if (!bug)
        {
            Player.RequestUnequipBySlot(slot);
            // Nota: Asegúrate que RequestUnequipBySlot en Player use la lógica correcta o llama a:
            // Player.RequestEquipItem(slot, -1);
        }
        // ------------------

        if (bug == false)
        {
            equipeArmor = null;
            Player.armor -= OriginalItem.itemData.Armadura;
        }
        OriginalItem.equiped = false;
        OriginalItem.inventory.capacity += OriginalItem.itemData.sizeX * OriginalItem.itemData.sizeY;
        OriginalItem.canvas.capacity1.text = OriginalItem.inventory.capacity.ToString();
        if (bug == true)
        {
            DraggableItemUI NewItem = transformNuevo.GetComponent<DraggableItemUI>();
            float armorDiference = OriginalItem.itemData.Armadura - NewItem.itemData.Armadura;
            Player.armor += armorDiference;
        }
        OriginalItem.canvas.armorText.text = Player.armor.ToString("F2");
        canvas.GetComponent<InventoryUI>().AddItemRT(transformOriginal);
        Player.GetComponent<Inventory>().AddItem(transformOriginal.GetComponent<DraggableItemUI>().itemData);
        if (slot == TipoEquipo.Pants || slot == TipoEquipo.Chest || slot == TipoEquipo.GloveR || slot == TipoEquipo.GloveL || slot == TipoEquipo.Helmet || slot == TipoEquipo.BootL || slot == TipoEquipo.BootR || slot == TipoEquipo.Belt)
        {
            Player.armorEquiped.Remove(OriginalItem);
        }
        else if (slot == TipoEquipo.Weapon)
        {
            Player.swordEquiped = null;
        }
        else if (slot == TipoEquipo.Tourch)
        {
            Player.torchEquiped = null;
        }
        OriginalItem.SetNormalMeasures();
    }
}
