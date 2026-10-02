using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    private int[,] grid;
    public int maxCapacity = 50;
    public int capacity = 0;
    public List<ItemData> items = new List<ItemData>(); //Lista de items en el inventario
    public Canvas_Controller canvas;
    void Start()
    {
        maxCapacity = 50 + (int)RunManager.capacity;
        canvas.capacity2.text = maxCapacity.ToString();
    }
    public bool CheckSpace(ItemData item)
    {

        if (capacity + (item.sizeX * item.sizeY) <= maxCapacity && item.quantity <= 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void FillSpace(ItemData item)
    {
        capacity += item.sizeX * item.sizeY;
        canvas.capacity1.text = capacity.ToString();
    }
    public void AddItem(ItemData item)
    {
        items.Add(item);
    }
    public void DeleteItem(ItemData item)
    {
        items.Remove(item);
    }
}