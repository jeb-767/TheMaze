using UnityEngine;
using UnityEngine.UI;
public class Map : MonoBehaviour
{
    public GameObject map; // Referencia al Canvas que deseas mostrar/ocultar

    private bool isCanvasActive = false; 
    [Header("Configuración de Cámara")]
    public Camera mapCamera;
    public bool isRotationLocked = false;
    private Quaternion lockedRotation;
    public Transform player;
    public float height = 100f; // Altura fija para la cámara del mapa
    //public GridLayoutGroup gridLayoutGroup; // Referencia al GridLayoutGroup para organizar los elementos del mapa
    void Start()
    {
        isCanvasActive = true;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        UpdateMapSize(5 + StatsPlayerManager.mapLv); // Establece el tamaño inicial del mapa
    }
    public void Update()
    {
    }
    void LateUpdate()
    {
        // Si la rotación está bloqueada, forzamos siempre la misma rotación
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, height, player.position.z);
        }
    }

    // Método para cambiar el tamaño (FOV)
    public void UpdateMapSize(float newFOV)
    {
        if (mapCamera != null)
        {
            mapCamera.orthographicSize = newFOV;
        }
    }
    // Método para cambiar el tamaño (FOV)
    public void UpdateMapSize()
    {
        float newFOV = mapCamera.orthographicSize + 1;
        if (mapCamera != null)
        {
            mapCamera.orthographicSize = newFOV;
        }
    }
    // Método para bloquear/desbloquear
    public void SetRotationLock(bool lockState)
    {
        isRotationLocked = lockState;
        if (isRotationLocked)
        {
            lockedRotation = transform.rotation; // Captura la rotación actual al bloquear
        }
    }
}