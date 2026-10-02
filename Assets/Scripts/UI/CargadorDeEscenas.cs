using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.UI;
using Unity.Netcode;

public class CargadorDeEscenas : MonoBehaviour // <-- ¡Ahora es un MonoBehaviour normal!
{
    [Header("Elementos de UI")]
    public GameObject pantallaDeCarga;
    public Slider barraDeProgreso;
    public TextMeshProUGUI consejosText;

    private void Start()
    {
        // 1. Lo protegemos para que no se destruya
        DontDestroyOnLoad(gameObject);
        pantallaDeCarga.SetActive(false);

        // 2. Nos suscribimos a los eventos de arranque del servidor/cliente
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += PrepararEventosDeRed;
            NetworkManager.Singleton.OnClientStarted += PrepararEventosDeRed;
        }
        consejosText.text = ObtenerConsejoAleatorio();
    }
    public string ObtenerConsejoAleatorio()
    {
        string[] consejos = new string[]
        {
            "Remember to check your surroundings for hidden paths!",
            "Use Ctrl to crouch and reset stamina faster.",
            "Some walls can be broken",
            "Keep an eye on your stamina bar during chases.",
            "Listen for audio cues to detect nearby enemies.",
            "Be cautious with the ghosts",
            "Pick swords and armor to increase your chances of survival.",
            "Use the environment to your advantage during combat.",
        };

        int indiceAleatorio = Random.Range(0, consejos.Length);
        return consejos[indiceAleatorio];
    }
    private void OnDestroy()
    {
        // Limpiamos los eventos si por alguna razón este objeto se destruye
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= PrepararEventosDeRed;
            NetworkManager.Singleton.OnClientStarted -= PrepararEventosDeRed;
            
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= GestionarEventosDeEscena;
            }
        }
    }

    private void PrepararEventosDeRed()
    {
        if (NetworkManager.Singleton.SceneManager != null)
        {
            // Desuscribimos primero por seguridad (evita doble suscripción) y volvemos a suscribir
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= GestionarEventosDeEscena;
            NetworkManager.Singleton.SceneManager.OnSceneEvent += GestionarEventosDeEscena;
        }
    }

    // Puedes llamar a esta función desde el ClientRpc de tu RoomManager
    public void ActivarPantallaCarga()
    {
        pantallaDeCarga.SetActive(true);
        Debug.Log("Pantalla de carga activada para el cliente: ");
        if (barraDeProgreso != null) barraDeProgreso.value = 0f;
        //StartCoroutine(SimularBarraProgreso());
    }

    // --- FUNCIÓN PARA EL SERVIDOR ---
    public void CambiarEscenaMultijugador(string nombreDeLaNuevaEscena)
    {
        // Como ya no es NetworkBehaviour, usamos el Singleton para saber si somos el Host
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(nombreDeLaNuevaEscena, LoadSceneMode.Single);
        }
    }

    // --- ESCUCHA DE EVENTOS ---
    private void GestionarEventosDeEscena(SceneEvent eventoDeEscena)
    {
        // Solo nos importan los eventos de NUESTRO cliente
        if (eventoDeEscena.ClientId != NetworkManager.Singleton.LocalClientId) return;

        switch (eventoDeEscena.SceneEventType)
        {
            case SceneEventType.Load:
                ActivarPantallaCarga();
                break;

            case SceneEventType.LoadComplete:
                // La escena terminó de cargar, apagamos todo
                pantallaDeCarga.SetActive(false);
                if (barraDeProgreso != null) barraDeProgreso.value = 0;
                break;
        }
    }

    private IEnumerator SimularBarraProgreso()
    {
        if (barraDeProgreso == null) yield break;

        float progresoFalso = 0f;
        // La barra subirá poco a poco hasta el 95% mientras la pantalla siga activa
        while (pantallaDeCarga.activeSelf) 
        {
            progresoFalso += Time.deltaTime * 0.8f; // Velocidad
            if (progresoFalso > 0.95f) progresoFalso = 0.95f; 
            
            barraDeProgreso.value = progresoFalso;
            yield return null;
        }
    }
}