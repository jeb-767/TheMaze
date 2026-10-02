using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;

[CreateAssetMenu(menuName = "Database/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<GameObject> allPrefabs;
}
