using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
public class CraftSetup : MonoBehaviour
{
    public ItemDataBase itemToCraft;
    public ItemDataBase itemForCrafting;
    public ItemData itemForCraft1;
    public ItemData itemForCraft2;
    public Image itemForCraftingImage;
    public Image itemForCraftingImage2;
    public TextMeshProUGUI quantityText1;
    public TextMeshProUGUI quantityText2;

    // Referencia directa para evitar FindObjectOfType
    public ItemIconGenerator generator;
    public int quantityNeeded;
    public int quantityNeeded2;
    public int quantityAvailable;
    void Start()
    {
        generator = FindObjectOfType<ItemIconGenerator>();
        quantityNeeded = Random.Range(1, quantityAvailable);
        quantityNeeded2 = quantityAvailable - quantityNeeded;
        quantityText1.text = quantityNeeded.ToString();
        quantityText2.text = quantityNeeded2.ToString();
        int random1 = Random.Range(0, itemForCrafting.objects.Count);
        itemForCraftingImage.sprite = generator.GenerateIcon(itemForCrafting.objects[random1].prefab, itemForCrafting.objects[random1]);
        int random2 = random1;
        itemForCraft1 = itemForCrafting.objects[random1];
        while (random2 == random1)
        {
            random2 = Random.Range(0, itemForCrafting.objects.Count);
        }
        itemForCraft2 = itemForCrafting.objects[random2];
        itemForCraftingImage2.sprite = generator.GenerateIcon(itemForCrafting.objects[random2].prefab, itemForCrafting.objects[random2]);
    }
}
