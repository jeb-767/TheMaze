using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.Cinemachine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class Player : NetworkBehaviour
{
    // --- VARIABLES SINCRONIZADAS ---
    public NetworkVariable<float> netHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> netStamina = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> netXRotation = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // LISTA DE EQUIPO SINCRONIZADA
    // Índices: 0=Weapon, 1=Torch, 2=Helmet, 3=Chest, 4=Pants, 5=BootsL, 6=BootsR, 7=GloveL, 8=GloveR, 9=Belt
    public NetworkList<int> netEquipmentSlots;
    public int savedWeaponID = -1; // Para recordar el arma equipada al morir y volver a equiparla al revivir
    public int savedTorchID = -1; // Para recordar la antorcha equipada al morir y volver a equiparla al revivir
    public bool isHoldingMap = false; // Para controlar el estado del mapa

    [Header("Movement Settings")]
    [SerializeField] public float mapSpeed  = 0f ;
    [SerializeField] public float walkSpeed = 1f ;
    [SerializeField] public float runSpeed = 2f;
    [SerializeField] public float sprintSpeed = 3f;
    public float currentSpeed = 0;
    [SerializeField] private float jumpHeight = 9f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] public int mouseSensitivity = 75;
    [SerializeField] public Transform playerCamera;
    [SerializeField] private float maxLookAngle = 90f;

    // Referencias Internas
    private CharacterController controller;
    public Vector3 velocity;
    public Vector3 move;
    public bool isGrounded;
    private float xRotation = 0f;
    private float lastYRotation;

    // Combate / Items
    public float maxHealthBase = 100f;
    public float InteractRange = 3f;
    public float armor = 0f; // Esta variable la gestiona EquipmentSlot.cs localmente
    public bool isDead = false;
    public bool Invencible = false;
    private bool isAtacking = false;
    private bool sword = false;
    public float DañoAct = 5f;
    public float MaxStamina = 100f;

    // Listas y Referencias
    // IMPORTANTE: Esta lista contendrá los GameObjects 3D (Espadas, Cascos, etc.) que lleva el muñeco
    public List<GameObject> allEquipmentObjects = new List<GameObject>();

    // Referencias UI (Mantenidas para compatibilidad con tus scripts de UI)
    public List<GameObject> keys = new List<GameObject>();
    public List<DraggableItemUI> armorEquiped = new List<DraggableItemUI>(); // Usado por EquipmentSlot
    public DraggableItemUI swordEquiped, torchEquiped; // Usado por EquipmentSlot

    // Componentes Visuales / Audio
    public Animator anim;
    public Camera RenderCamera;
    public Light lighting;
    public BoxCollider attackCollider;
    public BoxCollider handCollider;
    public ParticleSystem SlahEffect;
    public ParticleSystem handParticle;
    public AudioSource Source_Steps, Source_Jump, Source_Attack, damageSource, deadSource, resetStaminaSource, resetHeadSource, handSource, effortSource, equipSource, unequipSource, breakSource;
    public List<AudioSource> enableSounds = new List<AudioSource>(); //0 = step , 1 = health , 2 = stamina
    // UI y Otros
    public Canvas_Controller canvas;
    public GameObject HealthBar, StaminaBar, ExperienceBar, InventoryPanel , statsPanel, bodyRender , faceParts , armorParts , handsParts;
    public Inventory inventory;
    public InventoryUI ui;
    public Maze_Generator maze;
    public GameObject playerDead;

    private Coroutine StaminaReset;
    private Coroutine HealthReset;
    private List<int> idles = new List<int> { 1, 2, 3 };
    private int currentIdle;
    private LayerMask layerMask;
    private RaycastHit hit;
    [Header("Spectator Mode")]
    public NetworkVariable<bool> isEscaped = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Player spectateTarget; // A quién estamos mirando
    public float revive = 0f;
    public float lightRange = 0.9f;
    public float xp = 0f;
    public float maxXp = 100f;
    public int level = 1;
    public float attackVelocity = 1f;
    public GameObject mapCam;
    public int levelAvailable = 0;
    //Cofres
    public ChestSetup currentOpenChest;
    
    private void Awake()
    {
        maxHealthBase += RunManager.health;
        MaxStamina += RunManager.stamina;
        SetupVelocity();
        lightRange += RunManager.vision;
        revive += RunManager.revive;
        armor += RunManager.armor;
        netEquipmentSlots = new NetworkList<int>();
        HealthBar.GetComponent<Slider>().maxValue = maxHealthBase;
        StaminaBar.GetComponent<Slider>().maxValue = MaxStamina;
        attackVelocity = attackVelocity + 1 * RunManager.attackVelocity;
    }

    public override void OnNetworkSpawn()
    {
        // 1. AUTO-LLENADO DE LISTA DE OBJETOS 3D
        // Busca todos los hijos que tengan el script ItemDataPlayer (Tus objetos 3D)
        allEquipmentObjects.Clear();
        ItemDataPlayer[] foundItems = GetComponentsInChildren<ItemDataPlayer>(true);
        foreach (var item in foundItems) allEquipmentObjects.Add(item.gameObject);

        // 2. INICIALIZAR LISTA DE RED (Solo Server)
        if (IsServer)
        {
            // Creamos 14 huecos (para cubrir todos los valores de TipoEquipo)
            if (netEquipmentSlots.Count == 0)
            {
                for (int i = 0; i < 15; i++) netEquipmentSlots.Add(-1);
            }
            /*maze = FindObjectOfType<Maze_Generator>();
            StartCoroutine(MovePlayer()); */
        }

        // 3. BUSQUEDA AGRESIVA DE CÁMARAS Y BRAINS (Corrección cámara loca)
        Camera[] allCameras = GetComponentsInChildren<Camera>(true);
        CinemachineBrain[] brains = GetComponentsInChildren<CinemachineBrain>(true);
        Canvas_Controller[] canvasControllers = GetComponentsInChildren<Canvas_Controller>(true);
        Inventory[] inventories = GetComponentsInChildren<Inventory>(true);
        InventoryUI[] inventoryUIs = GetComponentsInChildren<InventoryUI>(true);
        AudioListener listener = GetComponentInChildren<AudioListener>();
        Collider[] colliders = GetComponentsInChildren<Collider>(true);

        if (IsOwner)
        {
            // --- SOY YO (El dueño) ---
            // Asignamos la cámara principal
            foreach (Camera cam in allCameras)
            {
                cam.gameObject.SetActive(true);
                cam.enabled = true;
                // Distinguir entre cámara principal y de renderizado por Layer o Tag
                //playerCamera = cam.transform;
                //RenderCamera = cam;
            }
            // --- SOLUCIÓN AL CONFLICTO CINEMACHINE ---
            // Si usamos movimiento manual (HandleMouseLook), el Brain debe estar APAGADO
            // para que no bloquee la rotación del ratón.
            foreach (var brain in brains)
            {
                brain.enabled = false;
                // Opcional: Destroy(brain); si estás seguro de que no lo usas para cinemáticas.
            }
            foreach (Collider coll in colliders)
            {
                coll.enabled = true;
            }
            // Activar el oído
            if (listener != null) listener.enabled = true;
            // Apagar la cámara "MainCamera" vieja de la escena si existe
            var sceneCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (sceneCamera != null)
            {
                bool esMiCamara = false;
                foreach (var c in allCameras) if (c.gameObject == sceneCamera) esMiCamara = true;
                if (!esMiCamara) sceneCamera.SetActive(false);
            }
            // Conectar UI y Configuración
            ConnectToUI();
            // Bloquear cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Vida Inicial
            if (IsServer) {netHealth.Value = maxHealthBase; netStamina.Value = MaxStamina;}
            else RequestInitialHealthServerRpc(GameManager.difficulty);
            if (Source_Attack) Source_Attack.enabled = true;
            if (Source_Jump) Source_Jump.enabled = true;
            RequestEnableSoundServerRpc(0 , true);
            RequestEnableSoundServerRpc(1 , false);
            RequestEnableSoundServerRpc(2 , false);
        }
        else
        {
            // --- ES OTRO (El personaje de tu amigo) ---
            // 1. Apagar Cámaras
            foreach (Camera cam in allCameras)
            {
                cam.enabled = false;
                cam.gameObject.SetActive(false);
            }
            // 2. Apagar AudioListener
            if (listener != null) listener.enabled = false;
            // 3. APAGAR CINEMACHINE BRAIN (Vital para que no usurpe la cámara principal)
            foreach (var brain in brains)
            {
                brain.enabled = false;
                brain.gameObject.SetActive(false);
            }
            foreach (var canvasCtrl in canvasControllers)
            {
                canvasCtrl.enabled = false;
                canvasCtrl.gameObject.SetActive(false);
            }
            foreach (var inv in inventories)
            {
                inv.enabled = false;
            }
            foreach (var invUI in inventoryUIs)
            {
                invUI.enabled = false;
                invUI.gameObject.SetActive(false);
            }
            foreach (Collider coll in colliders)
            {
                coll.enabled = false;
            }
            // 4. Apagar luces
            if (lighting != null) lighting.enabled = false;
            // 5. Poner en capa visual correcta
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
            //StartCoroutine(MovePlayer()); 
            RequestEnableSoundServerRpc(0 , false);
            RequestEnableSoundServerRpc(1 , false);
            RequestEnableSoundServerRpc(2 , false);
        }

        // Suscripciones
        netHealth.OnValueChanged += OnHealthChanged;
        netStamina.OnValueChanged += OnStaminaChanged;
        isEscaped.OnValueChanged += OnEscapedChanged;
        // SUSCRIPCIÓN A EQUIPO
        netEquipmentSlots.OnListChanged += OnEquipmentSlotChanged;

        // ACTUALIZACIÓN VISUAL INICIAL (Late Join)
        for (int i = 0; i < netEquipmentSlots.Count; i++)
        {
            UpdateVisualsForSlot(i, netEquipmentSlots[i]);
        }
    }

    public override void OnNetworkDespawn()
    {
        netHealth.OnValueChanged -= OnHealthChanged;
        netStamina.OnValueChanged -= OnStaminaChanged;
        netEquipmentSlots.OnListChanged -= OnEquipmentSlotChanged;
        isEscaped.OnValueChanged -= OnEscapedChanged;
    }
    private void OnEscapedChanged(bool oldVal, bool newVal)
    {
        if (newVal)
        {
            // Desactiva el movimiento y colisiones
            if (controller != null) controller.enabled = false;
            foreach (var col in GetComponentsInChildren<Collider>()) 
            {
                if(col.gameObject.name == "Trigger")
                {
                    continue;
                }
                else
                {
                    col.enabled = false;   
                }
            }
            
            // Apaga la malla/modelo 3D del jugador para que sea invisible
            foreach (var rend in GetComponentsInChildren<Renderer>()) rend.enabled = false;
        }
    }
    public IEnumerator MovePlayer()
    {
        yield return new WaitForSeconds(0.2f); 
        if (maze != null)
        {
            // El SERVIDOR calcula la posición
            Vector3 spawnPos = new Vector3(maze.entrance.x - Random.Range(3.5f, 2.5f), 0, maze.entrance.y);
            
            // El SERVIDOR llama al RPC para que el Cliente se mueva
            TeleportPlayerClientRpc(spawnPos);
        }
        else
        {
            Debug.LogError("No se encontró el Maze_Generator en el Servidor.");
        }
    }
    [ClientRpc]
    public void TeleportPlayerClientRpc(Vector3 targetPos)
    {
        if (IsOwner) // Solo el cliente dueño ejecuta esto
        {
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            
            transform.position = targetPos;
            
            if (cc != null) cc.enabled = true;
        }
    }
    private void Start()
    {
        Invencible = false;
        controller = GetComponent<CharacterController>();
        attackCollider = handCollider;
        restartCollider();
        SlahEffect = handParticle;
        Source_Attack = handSource;
        sword = false;
        if (anim) anim.SetBool("Sword", sword);
        anim.SetFloat("AttackVelocity", attackVelocity);
        revive = RunManager.revive;
        layerMask = LayerMask.GetMask("Interactable");
        lighting.intensity = lightRange;
        xp = 0f;
        StartCoroutine("RandomIdle");
        lastYRotation = transform.eulerAngles.y;
        controller.enabled = true;
        if (anim) anim.enabled = true;
        StartCoroutine("torchDurability");
        StartCoroutine(XPGain());
        if (IsOwner)
        {
            Debug.Log("Player Local Started");
        }
        Instantiate(mapCam, transform.position, Quaternion.Euler(0f, 0f, 0f));
    }

    // --- SISTEMA DE EQUIPAMIENTO RED ---

    // Esta es la función que llama tu EquipmentSlot.cs
    public void RequestEquipItem(TipoEquipo tipo, int itemID)
    {
        if (IsOwner)
        {
            int slotIndex = (int)tipo; // Conversión directa Enum -> int
            if (tipo == TipoEquipo.Map && itemID != -1)
            {
                isHoldingMap = true;

                // 1. Guardamos en la memoria qué IDs llevamos ahora mismo en arma y antorcha
                savedWeaponID = netEquipmentSlots[(int)TipoEquipo.Weapon];
                savedTorchID = netEquipmentSlots[(int)TipoEquipo.Tourch];

                // 2. Le decimos al servidor que esconda el arma y la antorcha (enviando un -1)
                if (savedWeaponID != -1) EquipItemServerRpc((int)TipoEquipo.Weapon, -1);
                if (savedTorchID != -1) EquipItemServerRpc((int)TipoEquipo.Tourch, -1);
            }

            // Enviamos al servidor directamente
            EquipItemServerRpc(slotIndex, itemID);
        }
    }

    // Función auxiliar para desequipar desde UI (Debes añadir la llamada en EquipmentSlot.cs si quieres sincronizar el desequipado)
    public void RequestUnequipBySlot(TipoEquipo tipo)
    {
        if (IsOwner)
        {
            EquipItemServerRpc((int)tipo, -1); // -1 para indicar que se ha desequipado
            if (tipo == TipoEquipo.Map)
            {
                isHoldingMap = false;

                // 1. Restauramos el arma si teníamos una guardada en la memoria
                if (savedWeaponID != -1) 
                {
                    EquipItemServerRpc((int)TipoEquipo.Weapon, savedWeaponID);
                    savedWeaponID = -1; // Limpiamos la memoria
                }
                else
                {
                    // Si no teníamos arma guardada, aseguramos que el jugador vuelva a su estado sin arma
                    EquipItemServerRpc((int)TipoEquipo.Weapon, -1);
                }

                // 2. Restauramos la antorcha si teníamos una guardada
                if (savedTorchID != -1) 
                {
                    EquipItemServerRpc((int)TipoEquipo.Tourch, savedTorchID);
                    savedTorchID = -1; // Limpiamos la memoria
                }
            }
        }
    }

    // Esta función la usa EquipmentSlot.cs para ocultar visualmente local,
    // pero la sobreescribimos para que mande la señal de red también.
    public void UnequipPlayer(GameObject objectToEquip)
    {
        if (!IsOwner) return;

        var data = objectToEquip.GetComponent<ItemDataPlayer>();
        if (data != null)
        {
            RequestUnequipBySlot(data.itemEquipment.tipoEquipo);
        }

        // La lógica visual local se ejecutará cuando vuelva la respuesta del servidor
    }

    [ServerRpc]
    private void EquipItemServerRpc(int slotIndex, int itemID)
    {
        if (slotIndex >= 0 && slotIndex < netEquipmentSlots.Count)
        {
            netEquipmentSlots[slotIndex] = itemID;
        }
    }

    // Evento que salta cuando cambia la lista (en todos los clientes)
    private void OnEquipmentSlotChanged(NetworkListEvent<int> changeEvent)
    {
        UpdateVisualsForSlot(changeEvent.Index, changeEvent.Value);
    }

    private void UpdateVisualsForSlot(int slotIndex, int itemID)
    {
        TipoEquipo targetType = (TipoEquipo)slotIndex;

        foreach (var obj in allEquipmentObjects)
        {
            if (obj == null) continue;

            var itemDataPlayer = obj.GetComponent<ItemDataPlayer>();
            // IMPORTANTE: Chequeo de seguridad por si itemEquipment está vacío
            if (itemDataPlayer == null || itemDataPlayer.itemEquipment == null) continue;

            var data = itemDataPlayer.itemEquipment;

            if (data.tipoEquipo == targetType)
            {
                // Activamos si el ID coincide, desactivamos si no
                bool shouldActivate = (data.id == itemID);
                obj.SetActive(shouldActivate);
                Debug.Log(data.id);

                if (shouldActivate)
                {
                    ApplyVisualEffects(obj, data);
                }
            }
        }

        // 4. Si es -1 (Desequipado), reseteamos animaciones específicas
        if (itemID == -1)
        {
            if (targetType == TipoEquipo.Weapon) ResetWeaponVisuals();
            if (targetType == TipoEquipo.Tourch) if (anim) anim.SetBool("Torch", false);
            handCollider.enabled = true;
            handCollider.gameObject.SetActive(true);
        }
    }

    private void ApplyVisualEffects(GameObject obj, ItemData itemEq)
    {
        // Lógica copiada y adaptada de tu EquipPlayer original
        if (itemEq.tipoEquipo == TipoEquipo.Weapon)
        {
            attackCollider = obj.GetComponent<BoxCollider>();
            if (obj.transform.childCount > 0)
                SlahEffect = obj.GetComponent<Transform>().GetChild(0).GetComponent<ParticleSystem>();

            Source_Attack = obj.GetComponent<AudioSource>();
            sword = true;
            if (anim) anim.SetBool("Sword", sword);
            if (equipSource) equipSource.Play();
        }
        else if (itemEq.tipoEquipo == TipoEquipo.Tourch)
        {
            if (anim) anim.SetBool("Torch", true);
        }
        attackCollider.enabled = false;
    }

    private void ResetWeaponVisuals()
    {
        attackCollider = handCollider;
        attackCollider.enabled = false;
        SlahEffect = handParticle;
        Source_Attack = handSource;
        sword = false;
        if (anim) anim.SetBool("Sword", false);
        DañoAct = 5f + RunManager.attack;
        if (unequipSource) unequipSource.Play();
    }

    // --- RESTO DE UPDATE Y LOGICA (Movimiento, Combate, etc) ---

    private void Update()
    {
        if (!IsOwner) 
        {
            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Euler(netXRotation.Value, 0f, 0f);
            }
            return;
        }
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        Attack();
        //RestartStamina();
        //if (IsServer) RestartHealth();
        // Raycast
        if (playerCamera != null)
        {
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, InteractRange, layerMask))
            {
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    if (canvas) canvas.enter_Interact();
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactObj.Interact(this);
                    }
                }
            }
            else if (canvas) canvas.normal();
        }


        Dead();

        // Caída
        if (transform.position.y < -10f  && !isDead)//|| transform.position.y >= 1.5f)
        {
            controller.enabled = false;
            transform.position = new Vector3(transform.position.x + 1, 0f, transform.position.z);
            controller.enabled = true;
        }
    }
    public void Dead()
    {
        // Muerte
        if (netHealth.Value <= 0 && !isDead)
        {
            if(revive > 0)
            {
                netHealth.Value = maxHealthBase * 0.5f;
                StartResetHealth();
                revive -= 1;
                return;
            }
            else
            {
                isDead = true;
                PreDeathSequence();
                RequestDeathServerRpc();
            }
        } 
    }
    private void LateUpdate()
    {
        if (IsOwner && isEscaped.Value)
        {
            HandleSpectator();
        }
    }

    [ServerRpc]
    private void RequestInitialHealthServerRpc(float difficulty)
    {
        netHealth.Value = maxHealthBase / (difficulty > 0 ? difficulty : 1);
    }

    [ServerRpc]
    public void RequestDamageServerRpc(float amount)
    {
        float finalDmg = amount - armor;
        if (finalDmg < 0) finalDmg = 0;
        netHealth.Value -= finalDmg;
    }

    [ServerRpc]
    private void RequestDeathServerRpc()
    {
        if (playerDead != null)
        {
            GameObject corpse = Instantiate(playerDead, transform.position, transform.rotation);
            corpse.GetComponent<NetworkObject>().Spawn();
        }
        //GetComponent<NetworkObject>().Despawn();
    }

    private void OnHealthChanged(float oldVal, float newVal)
    {
        if (IsOwner && HealthBar != null) HealthBar.GetComponent<Slider>().value = newVal;
        if (newVal < oldVal)
        {
            StartResetHealth();
            if (anim) anim.SetTrigger("Damaged");
        }
    }

    private void OnStaminaChanged(float oldVal, float newVal)
    {
        if (IsOwner && StaminaBar != null) StaminaBar.GetComponent<Slider>().value = newVal;
    }

    private void ConnectToUI()
    {

        if (canvas != null)
        {
            canvas.player = this; 
            if (HealthBar != null) HealthBar.GetComponent<Slider>().value = netHealth.Value;
            if (StaminaBar != null) StaminaBar.GetComponent<Slider>().value = netStamina.Value;
            if (canvas.armorText != null) canvas.armorText.text = armor.ToString("F2");
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }

    // --- INPUT HANDLERS ---
    private void HandleMovement()
    {
        if(isDead || isEscaped.Value) return;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        move = ((transform.right * x + transform.forward * z).normalized * currentSpeed);
        isGrounded = controller.isGrounded;
        if (anim) anim.SetBool("Grounded", isGrounded);
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        if (Input.GetKey(KeyCode.LeftShift) && z > 0 && netStamina.Value >= 2.5f)
        {
            currentSpeed = sprintSpeed;
            netStamina.Value -= 2.5f * Time.deltaTime;
            StartResetStamina();
        }
        else if (Input.GetKey(KeyCode.LeftControl)) currentSpeed = walkSpeed;
        else
        {
            if (netStamina.Value >= 1f)
            {
                currentSpeed = runSpeed;
                if (move != Vector3.zero)
                {
                    netStamina.Value -= 1f * Time.deltaTime;
                    StartResetStamina();
                }

            }
            else currentSpeed = walkSpeed;
        }
        if(canvas.mapOpened) currentSpeed = mapSpeed;
        controller.Move(move * Time.deltaTime);
        bool isMoving = isGrounded && move != Vector3.zero;
        if (enableSounds[0] != null) 
        {
            RequestEnableSoundServerRpc(0 , isMoving);
        }
        if (anim)
        {
            anim.SetBool("Idle", !isMoving);
            anim.SetFloat("Speed", controller.velocity.magnitude * 100f);
            Vector3 moveDirection = new Vector3(move.x, 0, move.z);
            anim.SetFloat("Direction", Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up));
        }
    }
    private IEnumerator ResetStamina()
    {
        yield return new WaitForSeconds(3f);
        RequestEnableSoundServerRpc(2 , true);
        // Ciclo de regeneración
        while (netStamina.Value < MaxStamina)
        {
            netStamina.Value += 1f + RunManager.staminaRegen; // O la cantidad que quieras
            if (netStamina.Value > MaxStamina) netStamina.Value = MaxStamina;

            yield return new WaitForSeconds(0.1f);
        }

        // Una vez lleno, limpiamos la referencia
        StaminaReset = null;
        RequestEnableSoundServerRpc(2 , false);
    }
    private IEnumerator ResetHealth()
    {
        yield return new WaitForSeconds(7f);
        RequestEnableSoundServerRpc(1 , true);
        while (netHealth.Value < maxHealthBase)
        {
            netHealth.Value += 0.2f + RunManager.healthRegen; // O la cantidad que quieras
            if (netHealth.Value > maxHealthBase)
            {
                netHealth.Value = maxHealthBase;
            }
            yield return new WaitForSeconds(0.1f);
        }
        HealthReset = null;
        RequestEnableSoundServerRpc(1 , false);
    }
    private void HandleJump()
    {
        if(isDead || isEscaped.Value) return;
        if (Input.GetButtonDown("Jump") && isGrounded && netStamina.Value >= 5f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -1f * gravity);
            if (anim) anim.SetTrigger("Jump");
            netStamina.Value -= 5f;
            StartResetStamina();
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if(isDead || isEscaped.Value) return;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
            if (playerCamera) playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            netXRotation.Value = xRotation;
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    private void Attack()
    {
        if (isHoldingMap) return;
        if (Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButtonDown(0) && !isAtacking)
        {
            if(Input.GetMouseButtonDown(0) && !isAtacking)
            {
                isAtacking = true;
                if (anim) anim.SetTrigger("Attack1");
                if (effortSource) effortSource.Play();
            } 
        }
    }

    public void PreDeathSequence()
    {
        if (anim) anim.SetBool("Dead", true);
        InventoryPanel.SetActive(true);
        if (ui != null) foreach (RectTransform item in ui.existingItems) if (item) item.GetComponent<DraggableItemUI>().leave(true);
        foreach (DraggableItemUI armor in armorEquiped) armor.leave(true);
        if (swordEquiped != null) swordEquiped.leave(true);
        if (torchEquiped != null) torchEquiped.leave(true);
        InventoryPanel.SetActive(false);
        if (canvas) canvas.death();
    }
    [ServerRpc]
    public void RequestDropItemServerRpc(uint prefabHash, Vector3 position, Quaternion rotation, float Damage, float Durability, float CurrentDurability, float Armor, int Cuantity, Rareza Quality)
    {
        // Buscamos el prefab en la lista global usando el Hash (uint)
        foreach (var networkPrefab in NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs)
        {
            if (networkPrefab.SourcePrefabGlobalObjectIdHash == prefabHash)
            {
                // El servidor instancia el objeto real
                GameObject itemDrop = Instantiate(networkPrefab.Prefab, position, rotation);

                // El servidor le da vida en la red para todos
                itemDrop.GetComponent<NetworkObject>().Spawn();
                if (itemDrop.GetComponent<Animator>())
                {
                    itemDrop.GetComponent<Animator>().SetBool("Floor", true);
                }
                if (itemDrop.GetComponent<Armor_Interact>())
                {
                    ItemData itemData = itemDrop.GetComponent<Armor_Interact>().newItem;
                    itemData.Daño = Damage;
                    itemData.durabilidad = Durability;
                    itemData.currentDurability = CurrentDurability;
                    itemData.Armadura = Armor;
                    itemData.quantity = Cuantity;
                    itemData.rarezaEquipo = Quality;
                }
                else if (itemDrop.GetComponent<TorchInteract>())
                {
                    ItemData itemData = itemDrop.GetComponent<TorchInteract>().newItem;
                    itemData.Daño = Damage;
                    itemData.durabilidad = Durability;
                    itemData.currentDurability = CurrentDurability;
                    itemData.Armadura = Armor;
                    itemData.quantity = Cuantity;
                    itemData.rarezaEquipo = Quality;
                }
                else if (itemDrop.GetComponent<ItemsDropInteract>())
                {
                    ItemData itemData = itemDrop.GetComponent<ItemsDropInteract>().newItem;
                    itemData.Daño = Damage;
                    itemData.durabilidad = Durability;
                    itemData.currentDurability = CurrentDurability;
                    itemData.Armadura = Armor;
                    itemData.quantity = Cuantity;
                    itemData.rarezaEquipo = Quality;
                }
                return;
            }
        }
        Debug.LogError($"[Netcode] No se encontró el prefab con Hash: {prefabHash}");
    }
    public void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.gameObject.TryGetComponent(out IInteractable interactObj))
        {
            interactObj.Interact(this);
        }
        if(!Invencible && (other.gameObject.tag == "Skeleton_Attack" || other.gameObject.tag == "Golem_Attack") || (other.gameObject.tag == "Boss_Attack") || (other.gameObject.tag == "Goblin_Attack"))
        {
            if(other.gameObject.tag == "Goblin_Attack")
            {
                other.gameObject.GetComponentInParent<Goblin_Movement>().StealItem();
                RequestDamageServerRpc(Random.Range(2f , 5) * GameManager.difficulty - armor);
            }
            else
            {
                RequestDamageServerRpc(Random.Range(10f , 20f) * GameManager.difficulty - armor);
            }
            Invencible = true;
            KnowBack(other.transform);
            StartCoroutine("restoreInvencibility");
        }
        if(other.gameObject.tag == "GhostCollider" && !isDead && !isEscaped.Value)
        {
            netHealth.Value = 0f;
            Debug.Log("Ghost Collider");
            other.gameObject.GetComponentInParent<Ghost_Movement>().Desapear();
        }
    }
    [ClientRpc]
    public void ReduceDurabilityClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Esto se ejecuta SOLO en el dueño del jugador (tu PC local)
        // Aquí SÍ puedes tocar la UI y el swordEquiped tranquilamente
        if (swordEquiped != null)
        {
            swordEquiped.itemData.currentDurability -= 1;
            swordEquiped.ChangeDurabilty();
            swordEquiped.Destroy();     
        }
    }
    // --- LÓGICA DEL MODO ESPECTADOR ---
    public void HandleSpectator()
    {
        // Si no tenemos a quién mirar, o nuestro objetivo ya escapó/murió, buscamos a otro
        if (spectateTarget == null || spectateTarget.isEscaped.Value || spectateTarget.isDead)
        {
            CycleSpectator();
        }

        // Pegar nuestra cámara a la del objetivo
        if (spectateTarget != null && spectateTarget.playerCamera != null)
        {
            playerCamera.position = spectateTarget.playerCamera.position;
            playerCamera.rotation = spectateTarget.playerCamera.rotation;
        }
    }

    public void CycleSpectator()
    {
        // Buscamos a todos los jugadores en la escena
        Player[] allPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        List<Player> validPlayers = new List<Player>();

        // Filtramos a los que sigan vivos y dentro del laberinto (y que no seamos nosotros)
        foreach (Player p in allPlayers)
        {
            if (!p.isEscaped.Value && !p.isDead && p != this)
            {
                validPlayers.Add(p);
                SetLayerRecursively(bodyRender, LayerMask.NameToLayer("Default"));
            }
        }

        // Si hay alguien a quien mirar, pasamos al siguiente en la lista
        if (validPlayers.Count > 0)
        {
            int currentIndex = validPlayers.IndexOf(spectateTarget);
            currentIndex = (currentIndex + 1) % validPlayers.Count;
            spectateTarget = validPlayers[currentIndex];
            SetLayerRecursively(spectateTarget.bodyRender, LayerMask.NameToLayer("OwnPlayer"));
            SetLayerRecursively(spectateTarget.faceParts, LayerMask.NameToLayer("OwnPlayer"));
            SetLayerRecursively(spectateTarget.armorParts, LayerMask.NameToLayer("OwnPlayer"));
            SetLayerRecursively(spectateTarget.handsParts, LayerMask.NameToLayer("Default"));
            xRotation = 0;
        }
        else
        {
            spectateTarget = null; // No queda nadie dentro
        }
    }

    // --- COMUNICACIÓN CON EL SERVIDOR ---
    [ServerRpc(RequireOwnership = false)]
    public void RequestEscapeServerRpc()
    {
        isEscaped.Value = true;
        statsPanel.SetActive(false);
        CheckAllPlayersEscaped();
    }

    private void CheckAllPlayersEscaped()
    {
        if (!IsServer) return;
        
        int escapedCount = 0;
        
        // Contamos cuántos clientes han activado "isEscaped"
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var clientObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (clientObj != null && clientObj.GetComponent<Player>().isEscaped.Value)
            {
                escapedCount++;
            }
        }

        // Si todos han escapado, forzamos el final de la partida
        if (escapedCount >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            EndGameForEveryoneClientRpc();
        }
    }

    [ClientRpc]
    private void EndGameForEveryoneClientRpc()
    {
        // Llamamos a tu función exit() del Canvas para volver al menú de forma segura
        if (canvas != null) canvas.exit();
    }
    public void ShowCoins(int num)
    {
        canvas.StartCoroutine(canvas.ShowCoinsEarned(num));
    }
    public void ShowXP(int num)
    {
        canvas.StartCoroutine(canvas.ShowExperienceEarned(num));
    }
    public void OnJump()
    {
        if (Source_Jump) Source_Jump.Play();
        effortSource.Play();
    }
    public void DeadEvent()
    {
        deadSource.Play();
    }
    public void DamageEvent()
    {
        if (damageSource) damageSource.Play();
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestEnableSoundServerRpc(int ind , bool val)
    {
        // 2. El servidor recibe el aviso y le ordena a TODOS los clientes que hagan ruido
        PlayEnableSoundClientRpc(ind , val);
    }

    // 3. Esta función se ejecuta en la computadora de cada uno de los jugadores
    [ClientRpc]
    public void PlayEnableSoundClientRpc(int ind , bool val)
    {
        enableSounds[ind].enabled = val;
    }

    public void levelUp()
    {
        level++;
        levelAvailable++;
        xp = 0f;
        maxXp += 20f; // Incrementa el XP necesario para el siguiente nivel
        ExperienceBar.GetComponent<Slider>().maxValue = maxXp;
        //RunManager.IncreaseStats(level); // Aumenta las estadísticas según el nuevo nivel
        if (canvas)
        {
            canvas.LevelUpIndicator();
        }
    }
    public void itemUpdated(ItemData item, bool add)
    {
        canvas.StartCoroutine(canvas.ShowItemsChanged(item, add));
    }
    // Auxiliares
    public void restartCollider() { attackCollider.enabled = false; isAtacking = false; }
    public void StartSoundAttack() { if (Source_Attack) Source_Attack.Play(); if (SlahEffect) SlahEffect.Play(); attackCollider.enabled = true;effortSource.Play(); }
    public void AddKey(GameObject key) { keys.Add(key); }
    public void Trap1() { StartCoroutine(Speed_Reduce(0.75f)); }
    public void Trap2() { StartCoroutine(Speed_Reduce(0.5f)); RequestDamageServerRpc(10 - armor); }
    public void KnowBack(Transform other) { StartCoroutine(DoKnockback()); }
    public void StartResetHealth() { if (HealthReset != null) {StopCoroutine("ResetHealth"); RequestEnableSoundServerRpc(1 , false);} HealthReset = StartCoroutine("ResetHealth"); }
    public void StartResetStamina() { if (StaminaReset != null){ StopCoroutine("ResetStamina"); RequestEnableSoundServerRpc(2 , false);} StaminaReset = StartCoroutine("ResetStamina"); }
    public void SetupVelocity() {walkSpeed = 1 + RunManager.velocity; runSpeed = 2 + RunManager.velocity; sprintSpeed = 3 + RunManager.velocity;}
    public void AddExperience(float amount) { xp += amount; ExperienceBar.GetComponent<Slider>().value = xp; if (xp >= maxXp) levelUp(); canvas.StartCoroutine(canvas.ShowExperienceEarned(amount));}
    public void InstantHeal(float amount) { netHealth.Value += amount; if (netHealth.Value > maxHealthBase) netHealth.Value = maxHealthBase; }
    private IEnumerator Speed_Reduce(float speed) { walkSpeed *= speed; runSpeed *= speed; sprintSpeed *= speed; yield return new WaitForSeconds(3); walkSpeed /= speed; runSpeed /= speed; sprintSpeed /= speed; }
    private IEnumerator DoKnockback() { float i = 0f; while (i <= 0.2f) { controller.Move(-transform.forward * Random.Range(2.8f, 3.2f) * Time.deltaTime); i += Time.deltaTime; yield return null; } }
    public IEnumerator torchDurability() { while (torchEquiped != null) { torchEquiped.itemData.currentDurability -= 1f; torchEquiped.ChangeDurabilty(); torchEquiped.Destroy(); yield return new WaitForSeconds(2f); } }
    public IEnumerator BreakItem(GameObject item) { if (breakSource) breakSource.Play(); yield return new WaitForSeconds(0.5f); Destroy(item); } // Simplificado
    private IEnumerator RandomIdle() { while (true) { currentIdle = idles[Random.Range(0, idles.Count)]; if (anim) anim.SetInteger("Idles", currentIdle); yield return new WaitForSeconds(Random.Range(3f, 30f)); } }
    private IEnumerator restoreInvencibility(){yield return new WaitForSeconds(0.5f); Invencible = false; Debug.Log("Invencibility:" + Invencible);}
    private IEnumerator XPGain() { while (true) { if (xp < maxXp) {xp += 0.5f + 1 * RunManager.xpGain; ExperienceBar.GetComponent<Slider>().value = xp;} else { xp = maxXp; levelUp(); StopCoroutine(XPGain()); } yield return new WaitForSeconds(1f); } }
}