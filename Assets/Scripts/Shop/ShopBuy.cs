using UnityEngine;

public class ShopBuy : MonoBehaviour
{
    public GameObject reloadB , keysB, mapB, compassB;
    public Menu menu;
    public void OnEnable()
    {
        reloadB.GetComponent<ShopSetup>().Setup((int)StatsPlayerManager.reloads , 5);
        keysB.GetComponent<ShopSetup>().Setup((int)GameManager.keys , 50);
        mapB.GetComponent<ShopSetup>().Setup((StatsPlayerManager.mapLv + 5) + " X " + (StatsPlayerManager.mapLv + 5), 30 * (StatsPlayerManager.mapLv + 1));
        if(StatsPlayerManager.hasCompass)
        {
            compassB.GetComponent<ShopSetup>().Setup("Owned");
        }
        else
        {
            compassB.GetComponent<ShopSetup>().Setup("Not Owned", 150);
        }
    }

    public void BuyReload()
    {
        if (GameManager.coins >= 5)
        {
            GameManager.coins -= 5;
            StatsPlayerManager.reloads += 1;
            menu.ChangeCoinsValue();
            SaveManager.GuardarPartida();
            reloadB.GetComponent<ShopSetup>().Setup((int)StatsPlayerManager.reloads , 5);
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }

    public void BuyKey()
    {
        if (GameManager.coins >= 50)
        {
            GameManager.coins -= 50;
            GameManager.keys += 1;
            menu.ChangeCoinsValue();
            SaveManager.GuardarPartida();
            keysB.GetComponent<ShopSetup>().Setup((int)GameManager.keys , 50);
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }

    public void BuyMap()
    {
        if (GameManager.coins >= 30 * (StatsPlayerManager.mapLv + 1) && StatsPlayerManager.mapLv < 5)
        {
            GameManager.coins -= 30 * (StatsPlayerManager.mapLv + 1);
            StatsPlayerManager.mapLv += 1;
            menu.ChangeCoinsValue();
            SaveManager.GuardarPartida();
            mapB.GetComponent<ShopSetup>().Setup((StatsPlayerManager.mapLv + 5) + " X " + (StatsPlayerManager.mapLv + 5), 30 * (StatsPlayerManager.mapLv + 1));
        }
        else
        {
            mapB.GetComponent<ShopSetup>().Setup((StatsPlayerManager.mapLv + 5) + " X " + (StatsPlayerManager.mapLv + 5), "MAX");
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void BuyCompass()
    {
        if(GameManager.coins >= 150 && !StatsPlayerManager.hasCompass)
        {
            GameManager.coins -= 150;
            StatsPlayerManager.hasCompass = true;
            menu.ChangeCoinsValue();
            SaveManager.GuardarPartida();
            compassB.GetComponent<ShopSetup>().Setup("Owned");
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
}
