using UnityEngine;

// Define los tipos de equipo que existen en tu juego.
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public string description;
    public GameObject prefab;   // El prefab 3D del objeto
    public int sizeX = 1;       // Ancho en casillas
    public int sizeY = 1;       // Alto en casillas
    public Sprite icon; // Se genera automáticamente
    public TipoEquipo tipoEquipo = TipoEquipo.None;
    public Rareza rarezaEquipo = Rareza.Common;
    public float Daño;
    public float Armadura;
    public float durabilidad;
    public float currentDurability;
    public bool equipable = false;
    public int quantity = 1;
}
public enum TipoEquipo
{
    Weapon,
    Chest,
    GloveR,
    GloveL,
    Helmet,
    BootR,
    BootL,
    Pants,
    Belt,
    Consumable,
    None,
    Key,
    Tourch,
    Dropped,
    Map,
}
public enum Rareza
{
    Common,
    Rare,
    Super_rare,
    Epic,
    Legendary,
    Mhytic,
}

