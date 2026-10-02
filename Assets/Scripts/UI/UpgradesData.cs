using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
public class UpgradesData : MonoBehaviour
{
    public RawImage[] levels = new RawImage[5];
    public Texture full;
    public TextMeshProUGUI coinsText;
    public int level = 0;
    public void Setup(int level ,int coins)
    {
        this.level = level;
        int i = 0;
        foreach (RawImage image in levels)
        {
            if(i < this.level)
            {
                image.texture = full;
            }
            else
            {
                break; 
            }
            i++;
        }
        coinsText.text = coins.ToString();
    }
    public void UpdateLevels(int i, int coins)
    {
        levels[i].texture = full;
        level++;
        coinsText.text = coins.ToString();
    }
    public void MaxLevel()
    {
        coinsText.text = "Max";
    }
}
