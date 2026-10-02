using UnityEngine;
using UnityEngine.UI;

public class compass : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El transform del jugador o la cámara principal que gira")]
    public Transform playerTransform;
    
    [Tooltip("El componente Raw Image de la brújula")]
    public RawImage compassImage;
    public GameObject compassPanel;
    public void Start()
    {
        if (StatsPlayerManager.hasCompass)
        {
            compassPanel.SetActive(true);
        }
        else
        {
            compassPanel.SetActive(false);
        }
    }
    void Update()
    {
        float currentYRotation = playerTransform.eulerAngles.y;
        float uvOffset = currentYRotation / 360f;
        compassImage.uvRect = new Rect(uvOffset, 0f, compassImage.uvRect.width, 1f);
    }
}