using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ShopSetup : MonoBehaviour
{
    public TextMeshProUGUI current, coinsN;
    public void Setup(int cuantity, int price)
    {
        current.text = "Current: " + cuantity.ToString(); 
        coinsN.text = price.ToString();
    }
    public void Setup(string cuantity, int price)
    {
        current.text = "Current: " + cuantity; 
        coinsN.text = price.ToString();
    }
    public void Setup(string cuantity, string price)
    {
        current.text = "Current: " + cuantity; 
        coinsN.text = price;
    }
    public void Setup(string cuantity)
    {
        current.text = "Current: " + cuantity; 
        coinsN.text = null;
    }
}
