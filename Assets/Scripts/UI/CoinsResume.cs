using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CoinsResume : MonoBehaviour
{
    public TextMeshProUGUI textCoins2, multiplier1, multiplier2 , multiplier3, textCoins3, textCoins1;
    void Start()
    {
        textCoins2.text = "+ " + RunManager.obtainedGold;  
        multiplier1.text = "X " + (1 + RunManager.goldGain);
        multiplier2.text = "X " + (1 + ((GameManager.size / 10) *2));
        multiplier3.text = "X " + (1 + ((GameManager.difficult / 10) *2));
        textCoins3.text = RunManager.Gold.ToString();
        textCoins1.text = "20";
        //(int)(20 + RunManager.Gold * (1 + ((GameManager.size / 10) *2)) * (1 + ((GameManager.difficult / 10) *2)) * (1 + RunManager.goldGain))
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
