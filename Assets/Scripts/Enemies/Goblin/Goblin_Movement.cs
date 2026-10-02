using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using Random = UnityEngine.Random;
public struct NetworkItemData : INetworkSerializable, IEquatable<NetworkItemData>
    {
        public int id;
        public TipoEquipo tipoEquipo;
        public Rareza rarezaEquipo;
        public float Daño;
        public float Armadura;
        public float durabilidad;
        public float currentDurability;
        public int quantity;

        // Serializamos las variables para que viajen por la red
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref tipoEquipo);
            serializer.SerializeValue(ref rarezaEquipo);
            serializer.SerializeValue(ref Daño);
            serializer.SerializeValue(ref Armadura);
            serializer.SerializeValue(ref durabilidad);
            serializer.SerializeValue(ref currentDurability);
            serializer.SerializeValue(ref quantity);
        }

    // Comparamos si dos items en la red son exactamente iguales
        public bool Equals(NetworkItemData other)
        {
            return id == other.id &&
                tipoEquipo == other.tipoEquipo &&
                rarezaEquipo == other.rarezaEquipo &&
                Daño == other.Daño &&
                Armadura == other.Armadura &&
                durabilidad == other.durabilidad &&
                currentDurability == other.currentDurability &&
                quantity == other.quantity;
        }
    }
public class Goblin_Movement : NetworkBehaviour
{

    public Goblin_Base_State currentState;
    public Goblin_Idle_State IdleState = new Goblin_Idle_State();
    public Goblin_Walk_State WalkState = new Goblin_Walk_State();
    public Goblin_Pursue_State PursueState = new Goblin_Pursue_State();
    public Goblin_Attack_State AttackState = new Goblin_Attack_State();
    public Goblin_Damage_State DamageSate = new Goblin_Damage_State();
    public Goblin_RunAway_State RunAwayState = new Goblin_RunAway_State();
    public Goblin_Dead_State DeadState = new Goblin_Dead_State();

    public Transform transform_Goblin;

    public Maze_Generator maze;
    public UnityEngine.AI.NavMeshAgent agent;

    public float currentSpeed;
    public Animator anim;

    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public GameObject player;

    public int number = 0;
    public NetworkVariable<float> Health = new NetworkVariable<float>(30f);
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(30f);
    public AudioSource audioSource, damageSource, deathSource, attackSource;
    public Rigidbody rb;
    public bool inmortal = false;
    [SerializeField] private BoxCollider attackCollider;
    private bool isDead = false;
    public GameObject itemToDrop;
    public NetworkList<NetworkItemData> stealedItem;
    public ItemDataBase itemDatabase;
    public ItemData originalItem;
    public bool Afk = false;
    private void Awake()
    {
        stealedItem = new NetworkList<NetworkItemData>();
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            Health.Value = Random.Range(25f, 45f) * GameManager.difficulty;
            attackCollider.enabled = false;
            currentHealth.Value = Health.Value;
            maze = GameObject.FindObjectOfType<Maze_Generator>();
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            emptyCells = maze.OriginalemptyCells;
            player = GameObject.FindWithTag("Player");
            currentState = IdleState;
            currentState.OnEnterState(this);
            audioSource.enabled = false;
            inmortal = false;
            maze.enemies.Add(this.gameObject);
        }
        maze = GameObject.FindObjectOfType<Maze_Generator>();

    }

    void Update()
    {
        if(!IsServer) return;
        if (currentHealth.Value <= 0f && !isDead)
        {
            isDead = true;
            SwitchState(DeadState);
            deathSource.Play();
            audioSource.enabled = false;
        }
        else if (!isDead)
        {
            currentState.OnUpdateState(this);
        }
        else
        {
            return;
        }
        /*if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            SwitchState(IdleState);
        }*/
    }

    public void SwitchState(Goblin_Base_State State)
    {
        currentState = State;
        State.OnEnterState(this);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            if (other.CompareTag("Player_Attack") && !inmortal && !isDead)
            {
                Player Attacker = other.GetComponentInParent<Player>(); 
                player = Attacker.gameObject;
                float plus = 1;
                if(Random.value < RunManager.critProbability)
                {
                    plus = 1 + RunManager.crit;
                }
                currentHealth.Value -= (Attacker.DañoAct / GameManager.difficulty) * plus;
                DraggableItemUI sword = Attacker.swordEquiped;
                Attacker.ReduceDurabilityClientRpc();
                if(currentState != RunAwayState)
                {
                    SwitchState(DamageSate);
                }
                damageSource.Play();
            }
            else
            {
                return;
            }
        }
    }
    public IEnumerator Dead()
    {
        int max = 1;
        this.agent.enabled = false;
        anim.SetTrigger("Die");
        yield return new WaitForSeconds(5f);
        maze.enemies.Remove(this.gameObject);
        
        if(Random.value < RunManager.luck)
        {
            max = 2;
        }
        
        // 1. Soltar el item normal que suelta el Goblin
        for(int i = 0; i < max; i++)
        {
            GameObject droppedItem = Instantiate(itemToDrop, transform.position, Quaternion.identity);
            droppedItem.GetComponent<NetworkObject>().Spawn();
        }
        RunManager.obtainedGold += (int)(this.Health.Value / 20);

        // 2. Soltar los items ROBADOS (Leyendo la red y buscando en la base de datos)
        foreach (NetworkItemData netItem in stealedItem)
        {
            // Buscamos el item original en tu ScriptableObject maestro
            Debug.Log("Buscando item con ID: " + netItem.id);
            originalItem = itemDatabase.GetItemByID(netItem.id);
            Debug.Log(originalItem);
            player.GetComponent<Player>().ShowCoins((int)(this.Health.Value / 20 * (1 + RunManager.goldGain)));
            player.GetComponent<Player>().AddExperience((int)(this.Health.Value / 10 * (1 + RunManager.xpGain)));
            this.GetComponent<NetworkObject>().Despawn();
            if (originalItem != null && originalItem.prefab != null)
            {
                // Instanciamos el Prefab original
                GameObject itemDrop = Instantiate(originalItem.prefab, transform.position, transform.rotation);
                itemDrop.GetComponent<NetworkObject>().Spawn();

                if (itemDrop.GetComponent<Animator>())
                {
                    itemDrop.GetComponent<Animator>().SetBool("Floor", true);
                }

                // Transferimos los stats que viajaron por la red a la instancia del suelo
                if (itemDrop.GetComponent<Armor_Interact>())
                {
                    ItemData itemData = itemDrop.GetComponent<Armor_Interact>().newItem;
                    itemData.Daño = netItem.Daño;
                    itemData.durabilidad = netItem.durabilidad;
                    itemData.currentDurability = netItem.currentDurability;
                    itemData.Armadura = netItem.Armadura;
                    itemData.quantity = netItem.quantity;
                    itemData.rarezaEquipo = netItem.rarezaEquipo;
                }
                else if (itemDrop.GetComponent<TorchInteract>())
                {
                    ItemData itemData = itemDrop.GetComponent<TorchInteract>().newItem;
                    Debug.Log(itemData);
                    /*itemData.Daño = netItem.Daño;
                    itemData.durabilidad = netItem.durabilidad;
                    itemData.currentDurability = netItem.currentDurability;
                    itemData.Armadura = netItem.Armadura;
                    itemData.quantity = netItem.quantity;
                    itemData.rarezaEquipo = netItem.rarezaEquipo;*/
                }
                else if (itemDrop.GetComponent<ItemsDropInteract>())
                {
                    ItemData itemData = itemDrop.GetComponent<ItemsDropInteract>().newItem;
                    itemData.Daño = netItem.Daño;
                    itemData.durabilidad = netItem.durabilidad;
                    itemData.currentDurability = netItem.currentDurability;
                    itemData.Armadura = netItem.Armadura;
                    itemData.quantity = netItem.quantity;
                    itemData.rarezaEquipo = netItem.rarezaEquipo;
                }
                Debug.Log("Objeto instanciado");
            }
        }

        // Limpiamos la lista por si acaso antes de destruir
        stealedItem.Clear();

        // 3. Otorgar recompensas al jugador
        
        Destroy(gameObject);
    }
    public IEnumerator ResetDMG()
    {
        yield return new WaitForSeconds(0.7f);
        inmortal = false;
    }
    public IEnumerator KnowBack()
    {
        agent.enabled = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        Vector3 dir = (transform.position - player.transform.position).normalized;
        dir.y = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * 100 * Time.deltaTime, ForceMode.Impulse);
        yield return new WaitForSeconds(0.3f);
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        agent.enabled = true;
        if(currentState != RunAwayState)
        {
            SwitchState(PursueState);
        }
    }
    public IEnumerator ResetAFK()
    {
        yield return new WaitForSeconds(25f);
        anim.SetBool("Idle", true);
        Afk = false;
        SwitchState(IdleState);
    }
    public void ResetAttackCollider()
    {
        attackCollider.enabled = false;

    }
    public void SetupAttackColliders()
    {
        attackSource.Play();
        attackCollider.enabled = true;
    }
    public void StealItem()
    {
        if (!IsServer) return;
        if(Afk) return;
        Player playerS = player.GetComponent<Player>();
        if(playerS.inventory.items.Count > 0)
        {
            int number = Random.Range(0, playerS.inventory.items.Count);
            ItemData itemRobado = playerS.inventory.items[number];

            // 1. Empaquetamos los datos compatibles con la red
            NetworkItemData netItem = new NetworkItemData
            {
                id = itemRobado.id,
                tipoEquipo = itemRobado.tipoEquipo,
                rarezaEquipo = itemRobado.rarezaEquipo,
                Daño = itemRobado.Daño,
                Armadura = itemRobado.Armadura,
                durabilidad = itemRobado.durabilidad,
                currentDurability = itemRobado.currentDurability,
                quantity = itemRobado.quantity
            };

            // 2. Lo guardamos en la lista sincronizada
            stealedItem.Add(netItem);

            // Lógica visual y de borrado del inventario
            playerS.inventory.capacity -= itemRobado.sizeX * itemRobado.sizeY;
            playerS.canvas.capacity1.text = playerS.inventory.capacity.ToString();
            playerS.inventory.DeleteItem(itemRobado);
            Destroy(playerS.ui.existingItems[number].gameObject);
            playerS.ui.DeleteItemRT(playerS.ui.existingItems[number]);
            SwitchState(RunAwayState);
            Afk = true;
            StartCoroutine("ResetAFK");
            player.GetComponent<Player>().itemUpdated(itemRobado , false);
        }
    }
}
