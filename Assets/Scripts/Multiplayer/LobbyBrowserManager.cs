using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
using UnityEngine.UI;

public class LobbyBrowserManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform lobbyListContainer;
    public GameObject lobbyItemPrefab;
    public TMP_InputField playerNameInput;
    public GameObject roomPanel;
    public GameObject waitPanel;
    public TMP_InputField roomNameInput;
    public static LobbyBrowserManager Instance;
    private string currentLobbyId;
    public static string currentLobbyCode = "";
    public TMP_InputField joinCodeInput;
    public bool privateGame = false; 
    public int numberOfPlayers = 4 , currentPlayers = 1;
    public Toggle privateGameToggle; // Nueva variable para marcar si la sala es privada o no
    public TMP_Dropdown numberOfPlayersDropdown; // Dropdown para seleccionar el número de jugadores
    public TMP_Text playersActive, playersMax;
    private void Awake()
    {
        // Configuramos el Singleton para poder llamarlo desde otros scripts
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Sesión iniciada. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al inicializar servicios: {e}");
        }
    }

    // --- FUNCIONES PARA EL HOST ---
    
    public void UI_CreateMatch() 
    {
        // Si no se pone nombre a la sala, generamos el mismo formato "Jugador_XXX"
        string fallbackName = "Jugador_" + Random.Range(0, 9999);
        string matchName = roomNameInput != null && !string.IsNullOrEmpty(roomNameInput.text) 
            ? roomNameInput.text 
            : fallbackName;

        // EL SECRETO ESTÁ AQUÍ: El Host guarda el nombre de la sala como su propio nombre
        PlayerPrefs.SetString("MyPlayerName", matchName);
        
        CreateMatch(matchName, numberOfPlayers, privateGame); // Aquí le pasamos el valor de si es privada o no
        playersActive.text = "1";
        playersMax.text = numberOfPlayers.ToString();
    }

    public async void CreateMatch(string matchName, int maxPlayers, bool isPrivate)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate, // <--- AQUÍ LE DECIMOS A UNITY SI ES PRIVADA
                Data = new Dictionary<string, DataObject>
                {
                    { "Size", new DataObject(DataObject.VisibilityOptions.Public, GameManager.size.ToString()) },
                    { "Difficulty", new DataObject(DataObject.VisibilityOptions.Public, GameManager.difficult.ToString()) },
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(matchName, maxPlayers, options);
            
            // Guardamos el ID para borrarla luego, Y EL CÓDIGO para mostrarlo en pantalla
            currentLobbyId = lobby.Id;
            currentLobbyCode = lobby.LobbyCode; 

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.StartHost();
            
            Debug.Log($"Lobby creado. Privado: {isPrivate}. Código para amigos: {currentLobbyCode}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al crear la partida: {e}");
        }
    }
    public void UI_JoinByCode()
    {
        if (joinCodeInput != null && !string.IsNullOrEmpty(joinCodeInput.text))
        {
            JoinPrivateLobby(joinCodeInput.text);
        }
    }
    private async void JoinPrivateLobby(string lobbyCode)
    {
        try
        {
            // Unity tiene una función específica para unirse mediante código
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            currentLobbyCode = joinedLobby.LobbyCode; // El cliente guarda el código
            numberOfPlayers = joinedLobby.MaxPlayers; // El cliente guarda el límite real de esta sala
            playersMax.text = numberOfPlayers.ToString();
            
            GameManager.size = int.Parse(joinedLobby.Data["Size"].Value);
            GameManager.difficult = int.Parse(joinedLobby.Data["Difficulty"].Value);

            string relayJoinCode = joinedLobby.Data["RelayCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            currentLobbyId = joinedLobby.Id; // Guardamos el ID por si nos salimos
            SaveClientNameLocally();

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            
            Debug.Log($"Unido a lobby privado mediante código: {lobbyCode}");
            roomPanel.SetActive(false);
            waitPanel.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al unirse por código. ¿Existe la sala?: {e}");
        }
    }

    // --- FUNCIONES PARA EL BUSCADOR (CLIENTE) ---
    
    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;
            /*options.Filters = new List<QueryFilter> {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            };*/

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            UpdateLobbyListUI(response.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error al buscar salas: {e}");
        }
    }

    private void UpdateLobbyListUI(List<Lobby> lobbies)
    {
        foreach (Transform child in lobbyListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbies)
        {
            GameObject newLobbyItem = Instantiate(lobbyItemPrefab, lobbyListContainer);
            PlayerUiPrefabList prefabScript = newLobbyItem.GetComponent<PlayerUiPrefabList>();
            prefabScript.name.text = lobby.Name;
            prefabScript.players.text = $"{lobby.Players.Count} / {lobby.MaxPlayers}";
            prefabScript.size.text = GameManager.GetSizeText(int.Parse(lobby.Data["Size"].Value));
            prefabScript.difficulty.text = GameManager.GetDifficultyText(int.Parse(lobby.Data["Difficulty"].Value));
            if(lobby.AvailableSlots == 0)
            {
                prefabScript.button.interactable = false;
                prefabScript.players.color = Color.red;

            }
            else
            {
                prefabScript.players.color = Color.green;
                prefabScript.button.interactable = true;
                prefabScript.button.onClick.AddListener(() => JoinSpecificLobby(lobby.Id));
            }
        }
    }

    private async void JoinSpecificLobby(string lobbyId)
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            currentLobbyId = joinedLobby.Id;

            currentLobbyCode = joinedLobby.LobbyCode; // El cliente guarda el código
            numberOfPlayers = joinedLobby.MaxPlayers; // El cliente guarda el límite real de esta sala
            playersMax.text = numberOfPlayers.ToString();

            GameManager.size = int.Parse(joinedLobby.Data["Size"].Value);
            GameManager.difficult = int.Parse(joinedLobby.Data["Difficulty"].Value);

            string relayJoinCode = joinedLobby.Data["RelayCode"].Value;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // Guardamos el nombre del cliente justo antes de conectar
            SaveClientNameLocally();

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            
            Debug.Log($"Unido a lobby '{joinedLobby.Name}' con código Relay: {relayJoinCode}");
            roomPanel.SetActive(false);
            waitPanel.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al unirse a la sala: {e}");
        }
    }

    private void SaveClientNameLocally()
    {
        // Si el cliente no escribe nombre, se le asigna "Jugador_XXX"
        string pName = playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text) 
            ? playerNameInput.text 
            : "Jugador_" + Random.Range(000, 9999);
            
        PlayerPrefs.SetString("MyPlayerName", pName);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }
    public async void LeaveUnityLobby()
    {
        try
        {
            if (!string.IsNullOrEmpty(currentLobbyId))
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(currentLobbyId, playerId);
                currentLobbyId = "";
                Debug.Log("Salimos del Lobby de Unity Services.");
                currentPlayers--;
                UpdatePlayerCount(currentPlayers); // Actualizamos el contador de jugadores al salir
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error al salir del lobby: {e}");
        }
    }
    // Añade esta función en LobbyBrowserManager.cs
    public async void DeleteUnityLobby()
    {
        try
        {
            if (!string.IsNullOrEmpty(currentLobbyId))
            {
                // Borra la sala completamente de los servidores de Unity
                await LobbyService.Instance.DeleteLobbyAsync(currentLobbyId);
                currentLobbyId = "";
                Debug.Log("La sala ha sido destruida porque el Host salió.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error al destruir el lobby: {e}");
        }
    }
    public void ChangeVisibility()
    {
        privateGame = privateGameToggle.isOn; // Actualizamos el valor según el toggle
        Debug.Log($"La sala ahora es {(privateGame ? "privada" : "pública")}.");
    }
    public void ChangeNumberOfPlayers()
    {
        if(numberOfPlayersDropdown.value == 0)
        {
            numberOfPlayers = 4;
        } 
        else
        {
            numberOfPlayers = numberOfPlayersDropdown.value + 1;
        }
        Debug.Log($"Número de jugadores establecido a: {numberOfPlayers}");
    }
    public void UpdatePlayerCount(int newCount)
    {
        currentPlayers = newCount;
        playersActive.text = currentPlayers.ToString();
        playersMax.text = numberOfPlayers.ToString();
    }
}