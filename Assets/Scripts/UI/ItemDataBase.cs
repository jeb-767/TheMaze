using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "Scriptable Objects/ItemDataBase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> objects = new List<ItemData>();
    public ItemData GetItemByID(int id)
    {
        foreach (ItemData item in objects)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null; // Si no lo encuentra
    }
    public int GetRandomItem()
    {
        int randomIndex = Random.Range(0, objects.Count);
        return randomIndex;
    }
}
