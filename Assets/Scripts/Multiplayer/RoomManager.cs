using UnityEngine;
using Unity.Netcode;
using TMPro; 
using UnityEngine.UI;

// 1. CREAMOS ESTA ESTRUCTURA PARA VINCULAR EL ID DE RED CON EL NOMBRE
public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public ulong ClientId;
    public Unity.Collections.FixedString32Bytes PlayerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
    }

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId && PlayerName == other.PlayerName;
    }
}

public class RoomManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public GameObject roomPanel;
    public GameObject lobbyPanel; // Añade aquí el panel principal (donde está la lista de lobbies) para volver a él
    public Transform playerListContainer; 
    public GameObject playerNamePrefab; 
    public Button startGameButton;
    public Button leaveButton; // Arrastra tu botón de Salir aquí en el Inspector
    public TextMeshProUGUI lobbyCodeText;
    public CargadorDeEscenas cargadorDeEscenas; 
    public SetupRunStats setup; // Referencia a tu script de stats para configurar al iniciar la partida
        // Arrastra tu script de cargador de escenas aquí

    // 2. CAMBIAMOS LA LISTA PARA QUE USE NUESTRA NUEVA ESTRUCTURA
    private NetworkList<PlayerData> playerNames;

    private void Awake()
    {
        playerNames = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        roomPanel.SetActive(true);

        leaveButton.onClick.AddListener(LeaveRoom);

        string myName = PlayerPrefs.GetString("MyPlayerName", "Jugador Desconocido");

        if (IsServer)
        {
            playerNames.Clear();
            startGameButton.gameObject.SetActive(true);
            startGameButton.onClick.AddListener(StartGame);
            // El Host añade su propio nombre con su ClientId
            AddPlayerName(NetworkManager.Singleton.LocalClientId, myName); 
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
            // El cliente envía su nombre por RPC
            RequestAddPlayerNameServerRpc(myName); 
        }
        if (lobbyCodeText != null)
        {
            lobbyCodeText.text = "Lobby Code: " + LobbyBrowserManager.currentLobbyCode;
        }
        // 3. NOS SUSCRIBIMOS A LOS CAMBIOS DE LA LISTA Y DESCONEXIONES
        playerNames.OnListChanged += UpdatePlayerListUI;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        
        UpdatePlayerListUI(new NetworkListEvent<PlayerData>());
    }

    public override void OnNetworkDespawn()
    {
        // Limpiamos los eventos al destruir/desactivar el objeto
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
        playerNames.OnListChanged -= UpdatePlayerListUI;
    }

    // --- LÓGICA DE AÑADIR JUGADORES ---
    [ServerRpc(RequireOwnership = false)]
    private void RequestAddPlayerNameServerRpc(string playerName, ServerRpcParams rpcParams = default)
    {
        // Obtenemos el ID del cliente que nos acaba de mandar este mensaje
        ulong clientId = rpcParams.Receive.SenderClientId;
        AddPlayerName(clientId, playerName);
    }

    private void AddPlayerName(ulong clientId, string name)
    {
        if (IsServer)
        {
            playerNames.Add(new PlayerData 
            { 
                ClientId = clientId, 
                PlayerName = new Unity.Collections.FixedString32Bytes(name) 
            });
        }
    }

    private void UpdatePlayerListUI(NetworkListEvent<PlayerData> changeEvent)
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in playerNames)
        {
            GameObject newNameObj = Instantiate(playerNamePrefab, playerListContainer);
            // Ahora accedemos al string a través de nuestra estructura
            newNameObj.GetComponent<PlayerUiPrefabList>().name.text = data.PlayerName.ToString();
        }
        LobbyBrowserManager.Instance.UpdatePlayerCount(playerNames.Count); // Actualizamos el contador de jugadores en el lobby browser
    }

    public void StartGame()
    {
        if (IsServer)
        {
            // 1. Avisamos a TODOS los clientes (incluido el Host) que enciendan su pantalla de carga
            ShowLoadingScreenClientRpc();
            
            // 2. Retrasamos el cambio de escena medio segundo. 
            // Esto da tiempo a que el mensaje viaje por la red y la UI se renderice antes de que Unity congele el juego para cargar.
            Invoke(nameof(LoadSceneNetwork), 0.5f);
        }
    }

    [ClientRpc]
    private void ShowLoadingScreenClientRpc()
    {
        setup.SetupStats();
        Debug.Log("Mostrando pantalla de carga...");
        
        // --- OPCIÓN A: Si usas tu script CargadorDeEscenas ---
        if (cargadorDeEscenas != null)
        {
            // Reemplaza esto con el método real que uses en tu script para activar la UI
            foreach (Transform item in this.transform)
            {
                item.gameObject.SetActive(false);
            }
            cargadorDeEscenas.ActivarPantallaCarga(); 
        }

        // --- OPCIÓN B: Si usas un GameObject simple (Panel) ---
        // Descomenta esto y añade un 'public GameObject loadingPanel;' al principio del script
        // if (loadingPanel != null) loadingPanel.SetActive(true);
    }

    private void LoadSceneNetwork()
    {
        // Netcode se encarga de sincronizar el cambio de escena para todos
        NetworkManager.Singleton.SceneManager.LoadScene("PrincipalScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    // --- LÓGICA DE SALIDA ---
    private void LeaveRoom()
    {
        if (IsServer)
        {
            // 1. SI SOMOS EL HOST: Destruimos el Lobby en Unity Services
            if (LobbyBrowserManager.Instance != null)
            {
                LobbyBrowserManager.Instance.DeleteUnityLobby();
            }
        }
        else
        {
            // 2. SI SOMOS CLIENTE: Solo liberamos nuestro asiento
            if (LobbyBrowserManager.Instance != null)
            {
                LobbyBrowserManager.Instance.LeaveUnityLobby();
            }
        }

        // Al hacer Shutdown, si somos el Host, Netcode expulsa a todos.
        // Si somos cliente, simplemente nos desconectamos.
        NetworkManager.Singleton.Shutdown();

        // Volvemos al menú
        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        roomPanel.SetActive(false);
        if(lobbyPanel != null) lobbyPanel.SetActive(true);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            // SI SOMOS HOST: Buscamos al cliente que se fue y lo borramos de la lista visual
            for (int i = 0; i < playerNames.Count; i++)
            {
                if (playerNames[i].ClientId == clientId)
                {
                    playerNames.RemoveAt(i);
                    break;
                }
            }
        }
        else
        {
            // SI SOMOS CLIENTE: 
            // Si el ID que se desconecta es el del Servidor (0) o somos nosotros mismos,
            // significa que el Host cerró la sala o perdimos conexión.
            if (clientId == NetworkManager.ServerClientId || clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("El Host ha cerrado la sala o te has desconectado.");
                ReturnToMenu();
            }
        }
    }
}