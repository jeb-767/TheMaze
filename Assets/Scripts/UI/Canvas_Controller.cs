using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

public class Canvas_Controller : NetworkBehaviour
{
    [Header("UI References")]
    public RawImage image;
    public Texture2D cursor;
    public Texture2D hand;
    public Texture2D hand_interact;
    public bool in_Interact = false;
    public bool isLevelSelection = false;
    // Referencia al Player Local (se llena automáticamente)
    public Player player;

    public GameObject InventoryPanel;
    private bool inventoryOpen = false;
    private bool pauseOpen = false;
    private bool settingOpen = false;
    public GameObject DeadPanel;
    public Maze_Generator maze;
    public GameObject Player_Prefab;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject finshPanel;
    public ItemIconGenerator imageGenerator;
    // Eliminamos la dependencia directa de "cameraObj" global, usaremos la del player
    public Slider slider;
    public TMP_InputField sensibilityText;
    public GameObject npc;
    public Player_Armor_Setup setup;
    public GameObject advise;
    public Slider healthSlider;
    public GameObject craftPanel;
    public GameObject exchangeTablePanel;
    public GameObject normalEquipmentPanel, reinforcePanel, fragmentPanel, coinsPanel, DialoguePanel , levelUpPanel , xpPanel, itemPanel, mapPanel, chestPanel, journalPanel, misionAdvisePanel, objectiveAdvisePanel, questPanel;
    public TextMeshProUGUI capacity1, capacity2, armorText, coinsText, dialogueText, xpText, itemQuantity, misionText, objectiveText, misionTypeText;
    public Image itemImage;
    public InventoryUI ui;
    public GenerateNewLevel levelGenerator;
    public GameObject mapLight;
    public bool mapOpened = false;
    public TextMeshProUGUI chestText;

    //Misiones
    public MissionManager missionManager;

    public Animator anim;

    public TextMeshProUGUI levelTextIndicator;
    public GameObject levelUpIndicator;
    public bool levelingUp = false;
    public override void OnNetworkSpawn()
    {
        // Nos suscribimos al evento de desconexión en cuanto el Canvas "nace" en la red
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnectedFromServer;
        }
        if(IsOwner)
        {
            // Solo el jugador local ejecuta esta lógica
            maze = GameObject.FindObjectOfType<Maze_Generator>();
        }
    }

    public override void OnNetworkDespawn()
    {
        // Limpiamos el evento para que no dé errores al destruir el Canvas
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnectedFromServer;
        }
    }

    private void OnDisconnectedFromServer(ulong clientId)
    {
        // Si nos echa el servidor (Host) o si nosotros mismos nos desconectamos
        if (clientId == NetworkManager.Singleton.LocalClientId || clientId == NetworkManager.ServerClientId)
        {
            Debug.Log("Desconectado de la partida. Volviendo al menú...");
            ReturnToMenuLocal();
        }
    }
    void Start()
    {
        // En multiplayer NO detenemos el tiempo
        Time.timeScale = 1;
        maze = GameObject.FindObjectOfType<Maze_Generator>();
        //maze = GameObject.FindObjectOfType<Maze_Generator>();

        // --- CORRECCIÓN CÁMARA LOCA ---
        // Eliminamos la búsqueda automática de "MainCamera" y la activación de CinemachineBrain.
        // Ahora confiamos en que el Player.cs gestione su propia cámara.

        DeadPanel.SetActive(false);
        inventoryOpen = false;
        image.texture = cursor;
        in_Interact = false;
        settingsPanel.SetActive(false);

        pausePanel.SetActive(false);
        StartCoroutine("DesactivatePanel");

        advise.gameObject.SetActive(false);
        craftPanel.SetActive(false);
        normalEquipmentPanel.SetActive(true);
        CloseFragmentTable();
        CloseReinforceTable();
        CloseExchangeTable();
        CloseCraftPanel();
        CloseJournal();
        if (healthSlider) healthSlider.value = 0;

        // Al inicio en el menú/lobby, cursor libre. Al jugar, Player.cs lo bloqueará.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        imageGenerator = FindObjectOfType<ItemIconGenerator>();
        mapLight = GameObject.FindGameObjectWithTag("MapLight");
        CloseMapPanel();
    }

    void Update()
    {
        // SEGURIDAD: Si el jugador no existe, no hacemos nada
        if (player == null) return;

        // UI Vida
        if (healthSlider != null)
        {
            healthSlider.maxValue = player.maxHealthBase;
            healthSlider.value = player.netHealth.Value;
        }
        if(isLevelSelection)
        {
            DisableCameraControl();
        }
        if(player.isDead || player.isEscaped.Value)
        {
            DisableCameraControl();
        }
        // --- INPUTS ---
        // La lógica de abrir/cerrar menús simplemente cambia el estado del cursor.
        // Player.cs detecta el cursor: si está visible, DEJA de mover la cámara.

        if (Input.GetKeyDown(KeyCode.I))
        {
            if(player.isDead || player.isEscaped.Value) return; // No abrir inventario si el jugador está muerto o ha escapado
            if (!inventoryOpen) OpenInventory();
            else hideInventory();
        }

        if(Input.GetKeyDown(KeyCode.J))
        {
            if(player.isDead || player.isEscaped.Value) return; // No abrir inventario si el jugador está muerto o ha escapado
            if (journalPanel.activeInHierarchy) CloseJournal();
            else OpenJournal();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(player.isDead || player.isEscaped.Value) return;
            if(mapOpened) CloseMapPanel();
            else if(chestPanel.activeInHierarchy) CloseChestPanel();
            // No abrir menú si el jugador está muerto o ha escapado
            else{
                if (!pauseOpen && !settingOpen) ShowPauseMenu();
                else if (pauseOpen) hidePauseMenu();
                else if (settingOpen) HideSettingMenu();
            }
            
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            if(player.isDead || player.isEscaped.Value || levelingUp) return; // No abrir inventario si el jugador está muerto o ha escapado
            OpenLevelUpPanel();
            
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if(mapOpened)
            {
                CloseMapPanel();
            }
            else
            {
                OpenMapPanel();
            }
        }
    }
    // --- GESTIÓN DE CURSOR Y CÁMARA ---
    // NOTA: Player.cs tiene "if (Cursor.lockState == CursorLockMode.Locked) { MoverCamara(); }"
    // Por tanto, para detener la cámara, SOLO necesitamos desbloquear el cursor.
    // No hace falta desactivar CinemachineBrains ni componentes de cámara.

    public void DisableCameraControl()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //player.playerCamera.GetComponent<CinemachineBrain>().enabled = false; // Desactivamos el Brain para evitar que intente mover la cámara mientras el cursor está libre
    }

    private void EnableCameraControl()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
       // player.playerCamera.GetComponent<CinemachineBrain>().enabled = true; // Desactivamos el Brain para evitar que intente mover la cámara mientras el cursor está libre
    }

    // --- FUNCIONES DE MENU ---

    public void ShowPauseMenu()
    {
        pauseOpen = true;
        pausePanel.SetActive(true);
        DisableCameraControl(); // Libera el cursor -> Player.cs deja de rotar

        // Opcional: Silenciar al jugador local mientras está en pausa
        /*if (player != null && player.GetComponent<AudioListener>())
            player.GetComponent<AudioListener>().enabled = false;*/
    }

    public void hidePauseMenu()
    {
        pauseOpen = false;
        pausePanel.SetActive(false);
        EnableCameraControl(); // Bloquea el cursor -> Player.cs vuelve a rotar

        /*if (player != null && player.GetComponent<AudioListener>())
            player.GetComponent<AudioListener>().enabled = true;*/
    }

    public void OpenInventory()
    {
        inventoryOpen = true;
        InventoryPanel.SetActive(true);
        normalEquipmentPanel.SetActive(true);
        chestPanel.SetActive(false);
        DisableCameraControl();
    }

    public void hideInventory()
    {
        inventoryOpen = false;
        InventoryPanel.SetActive(false);
        CloseReinforceTable();
        CloseFragmentTable();
        EnableCameraControl();
    }

    // --- INTERACCIÓN ---
    public void enter_Interact()
    {
        image.texture = hand;
    }

    public void normal()
    {
        if (in_Interact == true)
        {
            image.texture = hand_interact;
            StartCoroutine(ResetCursor());
        }
        else
        {
            image.texture = cursor;
        }
    }

    private IEnumerator ResetCursor()
    {
        yield return new WaitForSeconds(0.4f);
        image.texture = cursor;
        in_Interact = false;
    }

    // --- GAMEPLAY & UI ---

    public void Finish()
    {
        finshPanel.SetActive(true);
        // NOTA: Quitamos el DisableCameraControl() para que el jugador pueda seguir haciendo clic
        // y controlando si quiere. Si dejas el cursor libre, simplemente podrá hacer clic en pantalla.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameManager.completed[GameManager.size, GameManager.difficult] == 0)
        {
            GameManager.completed[GameManager.size, GameManager.difficult]++;
            GameManager.keys++;
        }
        else
        {
            GameManager.completed[GameManager.size, GameManager.difficult]++;
        }

        // ---> LA MAGIA: Le pedimos a nuestro jugador que informe al Servidor de que ha escapado <---
        if (player != null)
        {
            player.RequestEscapeServerRpc();
        }
        RunManager.Gold = (int)(20 + RunManager.obtainedGold * (1 + ((GameManager.size / 10) *2)) * (1 + ((GameManager.difficult / 10) *2)) * (1 + RunManager.goldGain));
        GameManager.coins += RunManager.Gold;
        RunManager.escaped = true;
        if(RunManager.usedReloads >= 3)
        {
            StatsPlayerManager.reloads = StatsPlayerManager.reloads - RunManager.usedReloads + 3 + RunManager.obtainedReloads;
        }
        StatsPlayerManager.reloads = RunManager.reloads - Mathf.Max( 0 , (RunManager.usedReloads - (3 + RunManager.obtainedReloads)));
        SaveManager.GuardarPartida();
    }

    public void QuitGame()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }

    public void death()
    {
        DeadPanel.SetActive(true);
        DisableCameraControl();
    }

    public void Respawn()
    {
        DeadPanel.SetActive(false);

        /*if (NetworkManager.Singleton.IsServer)
        {
            if (Player_Prefab != null && maze != null)
            {
                Vector3 spawnPos = new Vector3(maze.entrance.x - 3f, 0.8f, maze.entrance.y);
                GameObject newPlayer = Instantiate(Player_Prefab, spawnPos, Quaternion.identity);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
            }
        }
        else
        {
            exit(); // Cliente vuelve al menú
            return;
        }*/

        InventoryPanel.SetActive(true);
        StartCoroutine("DesactivatePanel");
        player.TeleportPlayerClientRpc(maze.spawnPos);
        player.netHealth.Value = player.maxHealthBase;
        player.isDead = false;
        player.anim.SetTrigger("Revive");
        EnableCameraControl();
    }

    public void exit()
    {
        DeadPanel.SetActive(false);

        // Apagamos la red completamente y destruimos el Manager para no solapar laberintos
        if(NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }

        ReturnToMenuLocal();
    }

    private void ReturnToMenuLocal()
    {
        // Desbloqueamos el cursor por si acaso estaba bloqueado antes de volver al menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Cargamos la escena del menú de forma LOCAL (sin usar Netcode)
        // OJO: Asegúrate de que "Menu" sea el nombre exacto de tu escena en las Build Settings
        if(!RunManager.escaped)
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            SceneManager.LoadScene("GameResume");
        }
    }
    public IEnumerator DesactivatePanel()
    {
        InventoryPanel.SetActive(true);
        normalEquipmentPanel.SetActive(true);
        yield return new WaitForSeconds(0.2f);

        if (player != null)
        {
            //layer.slots = GameObject.FindGameObjectsWithTag("EquipmentUI");
        }
        InventoryPanel.SetActive(false);
    }

    public void ChangeSensibility()
    {
        if (player == null) return;
        player.mouseSensitivity = (int)slider.value;
        sensibilityText.text = player.mouseSensitivity.ToString();
    }

    public void ChangeSensibility2()
    {
        if (player == null) return;

        int valorSensibilidad;
        bool esNumero = int.TryParse(sensibilityText.text, out valorSensibilidad);
        if (!esNumero)
        {
            sensibilityText.text = "75";
        }
        else
        {
            if (int.Parse(sensibilityText.text) >= 500) sensibilityText.text = "500";
            else if (int.Parse(sensibilityText.text) < 0) sensibilityText.text = "0";
        }
        slider.value = int.Parse(sensibilityText.text);
        player.mouseSensitivity = int.Parse(sensibilityText.text);
    }

    // --- PANELES ---

    public void ShowSettingMenu()
    {
        settingOpen = true;
        settingsPanel.SetActive(true);
        DisableCameraControl();
        pauseOpen = false;
        pausePanel.SetActive(false);
    }

    public void HideSettingMenu()
    {
        settingOpen = false;
        settingsPanel.SetActive(false);
        DisableCameraControl(); // Mantiene cursor visible porque volvemos a pausa (probablemente)
        pauseOpen = true;
        pausePanel.SetActive(true);
    }

    public void Yes()
    {
        if (npc)
        {
            // Buscamos la clase base (NPC_Talk) en lugar del script específico (Magician_Talk)
            NPC_Talk currentNPC = npc.GetComponent<NPC_Talk>();
            
            if(currentNPC != null)
            {
                // Se ejecutará el OnYes() específico del NPC con el que estés hablando
                currentNPC.OnYes();
            }
        }
    }

    public void No()
    {
        if (npc)
        {
            NPC_Talk currentNPC = npc.GetComponent<NPC_Talk>();
            
            if(currentNPC != null)
            {
                // Se ejecutará el OnNo() específico del NPC con el que estés hablando
                currentNPC.OnNo();
            }
        }
    }

    public void CloseCraftPanel()
    {
        craftPanel.SetActive(false);
        EnableCameraControl();
    }

    public void OpenCraftPanel()
    {
        craftPanel.SetActive(true);
        DisableCameraControl();
    }
    public void OpenExchangeTable()
    {
        exchangeTablePanel.SetActive(true);
        DisableCameraControl();
    }
    public void CloseExchangeTable()
    {
        exchangeTablePanel.SetActive(false);
        EnableCameraControl();
    }

    public void CloseReinforceTable()
    {
        if (player == null) return;

        if (player.torchEquiped != null) player.torchEquiped.gameObject.SetActive(true);
        if (player.swordEquiped != null) player.swordEquiped.gameObject.SetActive(true);
        foreach (DraggableItemUI item in player.armorEquiped)
        {
            if (item != null) item.gameObject.SetActive(true);
        }

        ReinfrocementSlot slot = reinforcePanel.transform.GetChild(1).GetComponent<ReinfrocementSlot>();
        if (slot != null && slot.actualItem != null)
        {
            slot.UnEquip(slot.actualItem, null, false);
            slot.actualItem.anchoredPosition = new Vector2(Random.Range(100f, 1250f), Random.Range(-100f, -730f));
            slot.actualItem = null;
        }
        normalEquipmentPanel.SetActive(true);
        reinforcePanel.SetActive(false);
        InventoryPanel.SetActive(false);
        EnableCameraControl();
    }

    public void OpenReinforceTable()
    {
        if (player == null) return;

        if (player.torchEquiped != null) player.torchEquiped.gameObject.SetActive(false);
        if (player.swordEquiped != null) player.swordEquiped.gameObject.SetActive(false);
        foreach (DraggableItemUI item in player.armorEquiped) if (item) item.gameObject.SetActive(false);

        InventoryPanel.SetActive(true);
        normalEquipmentPanel.SetActive(false);
        reinforcePanel.SetActive(true);
        fragmentPanel.SetActive(false);
        DisableCameraControl();
    }

    public void CloseFragmentTable()
    {
        if (player == null) return;

        if (player.torchEquiped != null) player.torchEquiped.gameObject.SetActive(true);
        if (player.swordEquiped != null) player.swordEquiped.gameObject.SetActive(true);
        foreach (DraggableItemUI item in player.armorEquiped) if (item) item.gameObject.SetActive(true);

        FragmentSlot slot = fragmentPanel.transform.GetChild(1).GetComponent<FragmentSlot>();
        if (slot != null && slot.actualItem != null)
        {
            slot.UnEquip(slot.actualItem, null, false);
            slot.actualItem.anchoredPosition = new Vector2(Random.Range(100f, 1250f), Random.Range(-100f, -730f));
            slot.actualItem = null;
        }
        normalEquipmentPanel.SetActive(true);
        fragmentPanel.SetActive(false);
        InventoryPanel.SetActive(false);
        EnableCameraControl();
    }
    public void OpenFragmentPanel()
    {
        if (player == null) return;

        if (player.torchEquiped != null) player.torchEquiped.gameObject.SetActive(false);
        if (player.swordEquiped != null) player.swordEquiped.gameObject.SetActive(false);
        foreach (DraggableItemUI item in player.armorEquiped) if (item) item.gameObject.SetActive(false);

        InventoryPanel.SetActive(true);
        normalEquipmentPanel.SetActive(false);
        reinforcePanel.SetActive(false);
        fragmentPanel.SetActive(true);
        DisableCameraControl();
    }

    public void OpenDialoguePanel(string text)
    {
        dialogueText.text = text;
        DialoguePanel.SetActive(true);
        DisableCameraControl();
    }

    public void CloseDialoguePanel()
    {
        DialoguePanel.SetActive(false);
        EnableCameraControl();
    }
    public void OpenLevelUpPanel()
    {
        levelingUp = true;
        levelUpPanel.SetActive(true);
        isLevelSelection = true;
        levelGenerator.Generate();
        DisableCameraControl();
    }
    
    public void CloseLevelUpPanel()
    {
        levelingUp = false;
        levelUpPanel.SetActive(false);
        EnableCameraControl();
        player.levelAvailable--;
        isLevelSelection = false;
    }

    public void ShowAdvise(string text, Color color)
    {
        StartCoroutine(AdviseCoroutine(text, color));
    }
    
    public void ChangeDialogueText(string text)
    {
        dialogueText.text = text;
    }
    public void OpenMapPanel()
    {
        mapPanel.SetActive(true);
        mapOpened = true;
        mapLight.SetActive(true);
        player.RequestEquipItem(TipoEquipo.Map, 999); // Equipamos el mapa (999 es un ID ficticio, el Player.cs solo mira el tipo para activar la animación)
    }
    public void CloseMapPanel()
    {
        mapPanel.SetActive(false);
        mapOpened = false;
        player.RequestUnequipBySlot(TipoEquipo.Map);
        mapLight.SetActive(false);
    }


    public IEnumerator AdviseCoroutine(string text, Color color)
    {
        if (advise != null)
        {
            advise.GetComponent<TextMeshProUGUI>().text = text;
            advise.GetComponent<TextMeshProUGUI>().color = color;
            advise.SetActive(true);
            yield return new WaitForSeconds(4);
            advise.SetActive(false);
        }
    }
    public IEnumerator ShowCoinsEarned(int quantity)
    {
        coinsPanel.SetActive(true);
        coinsText.text = "+" + quantity.ToString();
        yield return new WaitForSeconds(4f);
        coinsPanel.SetActive(false);
    }
    public IEnumerator ShowExperienceEarned(float quantity)
    {
        xpPanel.SetActive(true);
        xpText.text = "+" + quantity.ToString() + " XP";
        yield return new WaitForSeconds(4f);
        xpPanel.SetActive(false);
    }
    public IEnumerator ShowItemsChanged(ItemData item, bool add)
    {
        itemPanel.SetActive(true);
        if(add)
        {
            itemQuantity.text = "+" + item.quantity.ToString();
        }
        else
        {
            itemQuantity.text = "-" + item.quantity.ToString(); 
        }
        imageGenerator.DesactivateBackground();
        itemImage.sprite = imageGenerator.GenerateIcon(item.prefab , item);
        yield return new WaitForSeconds(4f);
        itemPanel.SetActive(false);
    }
    public void OpenChestPanel()
    {
        if (player.torchEquiped != null) player.torchEquiped.gameObject.SetActive(false);
        if (player.swordEquiped != null) player.swordEquiped.gameObject.SetActive(false);
        foreach (DraggableItemUI item in player.armorEquiped) if (item) item.gameObject.SetActive(false);
        InventoryPanel.SetActive(true);
        normalEquipmentPanel.SetActive(false);
        chestPanel.SetActive(true);
        DisableCameraControl();
    }
    public void CloseChestPanel()
    {
        if (player.torchEquiped != null) player.torchEquiped.gameObject.SetActive(true);
        if (player.swordEquiped != null) player.swordEquiped.gameObject.SetActive(true);
        foreach (DraggableItemUI item in player.armorEquiped) if (item) item.gameObject.SetActive(true);
        normalEquipmentPanel.SetActive(true);
        InventoryPanel.SetActive(false);
        chestPanel.SetActive(false);
        DisableCameraControl();
    }
    public void OpenQuestPanel()
    {
        questPanel.SetActive(true);
        DisableCameraControl();
    }
    public void CloseQuestPanel()
    {
        questPanel.SetActive(false);
        EnableCameraControl();
    }
    public void OpenJournal()
    {
        journalPanel.SetActive(true);
        DisableCameraControl();
    }
    public void CloseJournal()
    {
        journalPanel.SetActive(false);
        EnableCameraControl();
    }
    public void GiveMision(MisionsBase mision)
    {
        missionManager.AcceptMission(mision);
    }
    public IEnumerator ShowNewMisionAdvise(string misionName , bool isNew)
    {
        misionAdvisePanel.SetActive(true);
        if(isNew)
        {
            misionTypeText.text = "New Mission";
        }
        else
        {
            
            misionTypeText.text = "Mission Completed";
        }
        misionText.text = misionName;
        anim.SetBool("MisionsAdvice" , true);
        yield return new WaitForSeconds(3.5f);
        anim.SetBool("MisionsAdvice" , false);
        yield return new WaitForSeconds(0.5f);
        misionAdvisePanel.SetActive(false);
    }

    public IEnumerator ShowObjectiveAdvise(string objectiveName)
    {
        objectiveAdvisePanel.SetActive(true);
        objectiveText.text = objectiveName;
        yield return new WaitForSeconds(4);
        objectiveAdvisePanel.SetActive(false);
    }
    public void LevelUpIndicator()
    {
        levelUpIndicator.SetActive(true);
        levelTextIndicator.text = player.levelAvailable.ToString();
    }
}