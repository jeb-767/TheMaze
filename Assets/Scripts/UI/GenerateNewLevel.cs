using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GenerateNewLevel : MonoBehaviour
{
    public GameObject panelN, panelS;
    public LevelUpMenu N1 ,N2 , N3, S1, S2, S3 , S4;
    public Canvas_Controller canvas;
    public AudioSource newLevel;
    public TextMeshProUGUI reloadText;
    public void Generate()
    {
        reloadText.text = RunManager.reloads.ToString();
        panelS.SetActive(false);
        panelN.SetActive(false);
        newLevel.Play();
        if(Random.Range(0, 1/RunManager.luck) < 1)
        {
            panelS.SetActive(true);
            S1.Generate();
            S2.Generate();
            S3.Generate();
            S4.Generate();
        }
        else
        {
            panelN.SetActive(true);
            N1.Generate();
            N2.Generate();
            N3.Generate();
        }
    }

    public void CloseN()
    {
        N1.anim.SetTrigger("OUT");
        N2.anim.SetTrigger("OUT");
        N3.anim.SetTrigger("OUT");
        StartCoroutine(Close(false));
    }
    public void CloseS()
    {
        S1.anim.SetTrigger("OUT");
        S2.anim.SetTrigger("OUT");
        S3.anim.SetTrigger("OUT");
        S4.anim.SetTrigger("OUT");
        StartCoroutine(Close(true));
    }
    public IEnumerator Close(bool special)
    {
        yield return new WaitForSeconds(1f);
        if(special)
        {
            N1.anim.SetTrigger("Exit");
            N2.anim.SetTrigger("Exit");
            N3.anim.SetTrigger("Exit"); 
        }
        else
        {
            S1.anim.SetTrigger("Exit");
            S2.anim.SetTrigger("Exit");
            S3.anim.SetTrigger("Exit");
            S4.anim.SetTrigger("Exit"); 
        }
        panelS.SetActive(false);
        panelN.SetActive(false);
        canvas.CloseLevelUpPanel();
    }
    public void disableReload()
    {
        if(panelS.activeInHierarchy)
        {
            S1.reloadButton.image.color = Color.gray;
            S2.reloadButton.image.color = Color.gray;
            S3.reloadButton.image.color = Color.gray;
            S4.reloadButton.image.color = Color.gray;
        }
        else
        {
            N1.reloadButton.image.color = Color.gray;
            N2.reloadButton.image.color = Color.gray;
            N3.reloadButton.image.color = Color.gray; 
        }
    }
    public void enableReload()
    {
        if(panelS.activeInHierarchy)
        {
            S1.reloadButton.image.color = Color.white;
            S2.reloadButton.image.color = Color.white;
            S3.reloadButton.image.color = Color.white;
            S4.reloadButton.image.color = Color.white;
        }
        else
        {
            N1.reloadButton.image.color = Color.white;
            N2.reloadButton.image.color = Color.white;
            N3.reloadButton.image.color = Color.white; 
        }
    }

}
