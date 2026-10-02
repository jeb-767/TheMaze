using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;
using System.Linq;
public class Player_Armor_Setup : MonoBehaviour
{
    public Transform player;
    public Transform armor;
    public Transform sword;
    public Transform tourch;
    [SerializeField] public Dictionary<TipoEquipo, List<ItemDataPlayer>> parts = new Dictionary<TipoEquipo, List<ItemDataPlayer>>();
    public void Awake()
    {
        foreach (TipoEquipo slot in System.Enum.GetValues(typeof(TipoEquipo)))
        {
            parts[slot] = new List<ItemDataPlayer>();
        }
    }
    public void SetParts()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        armor = player.Find("ARMOR PARTS");
        sword = player.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Swords");
        tourch = player.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "Tourchs");
        if (armor != null)
        {
            // Casco
            Transform cascoParent = armor.Find("HEADS");
            if (cascoParent != null)
            {
                foreach (Transform child in cascoParent)
                {
                    RegisterPart(TipoEquipo.Helmet, child.GetComponent<ItemDataPlayer>());
                }
            }

            // Pechera
            Transform pecheraParent = armor.Find("CHESTS");
            if (pecheraParent != null)
            {
                foreach (Transform child in pecheraParent)
                {
                    RegisterPart(TipoEquipo.Chest, child.GetComponent<ItemDataPlayer>());
                }
            }

            Transform armsParent = armor.Find("ARMS");
            if (armsParent != null)
            {
                Transform rParent = armsParent.Find("R");
                Transform lParent = armsParent.Find("L");
                foreach (Transform child in rParent)
                {
                    RegisterPart(TipoEquipo.GloveR, child.GetComponent<ItemDataPlayer>());
                }
                foreach (Transform child in lParent)
                {
                    RegisterPart(TipoEquipo.GloveL, child.GetComponent<ItemDataPlayer>());
                }
            }
            Transform cinturonParent = armor.Find("BELTS");
            if (cinturonParent != null)
            {
                foreach (Transform child in cinturonParent)
                {
                    RegisterPart(TipoEquipo.Belt, child.GetComponent<ItemDataPlayer>());
                }
            }
            Transform pantalonesParent = armor.Find("LEGS");
            if (pantalonesParent != null)
            {
                foreach (Transform child in pantalonesParent)
                {
                    RegisterPart(TipoEquipo.Pants, child.GetComponent<ItemDataPlayer>());
                }
            }
            Transform feetParent = armor.Find("FEET");
            if (feetParent != null)
            {
                Transform rParent2 = feetParent.Find("R");
                Transform lParent2 = feetParent.Find("L");
                foreach (Transform child in rParent2)
                {
                    RegisterPart(TipoEquipo.BootR, child.GetComponent<ItemDataPlayer>());
                }
                foreach (Transform child in lParent2)
                {
                    RegisterPart(TipoEquipo.BootL, child.GetComponent<ItemDataPlayer>());
                }
            }

        }
        if (sword != null)
        {
            foreach (Transform child in sword)
            {
                RegisterPart(TipoEquipo.Weapon, child.GetComponent<ItemDataPlayer>());
            }
        }
        if (tourch != null)
        {
            foreach (Transform child in tourch)
            {
                RegisterPart(TipoEquipo.Tourch, child.GetComponent<ItemDataPlayer>());
            }
        }
    }
    void Start()
    {
        SetParts();
    }
    public void RegisterPart(TipoEquipo slot, ItemDataPlayer part)
    {
        parts[slot].Add(part);
        part.gameObject.SetActive(false); // desactivar al inicio
    }
}
