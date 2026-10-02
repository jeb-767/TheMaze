using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
public class OnEnterMouse : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    void Start()
    {
        image = GetComponent<Image>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPointerEnter(PointerEventData eventData)
    {
        Color color = image.color;
        color.a = 0.6f; // Cambia el valor alfa a 0.5 (50% de transparencia)
        image.color = color;
    }

    // Update is called once per frame
    public void OnPointerExit(PointerEventData eventData)
    {
        Color color = image.color;
        color.a = 1f; // Cambia el valor alfa a 0.5 (50% de transparencia)
        image.color = color;
    }
}
