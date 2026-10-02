using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
public class Golem_Movement : NetworkBehaviour
{

    public Golem_Base_State currentState;
    public Golem_Idle_State IdleState = new Golem_Idle_State();
    public Golem_Walk_State WalkState = new Golem_Walk_State();
    public Golem_Pursue_State PursueState = new Golem_Pursue_State();
    public Golem_Attack_State AttackState = new Golem_Attack_State();
    public Golem_Damage_State DamageSate = new Golem_Damage_State();


    public Golem_Dead_State DeadState = new Golem_Dead_State();


    public Transform transform_golem;
    public List<Vector3> wayponts = new List<Vector3>();

    public Maze_Generator maze;
    public UnityEngine.AI.NavMeshAgent agent;

    public float currentSpeed;
    public Animator anim;

    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public GameObject player;

    public int number = 0;
    public NetworkVariable<float> Health = new NetworkVariable<float>(30f);
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(30f);
    public AudioSource audioSource, damageSource, deathSource;
    public Rigidbody rb;
    public bool inmortal = false;
    [SerializeField] private BoxCollider attackCollider;
    private bool isDead = false;
    public GameObject itemToDrop;
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
            for (int i = 0; i < Random.Range(2, 5); i++)
            {
                int number = Random.Range(0, emptyCells.Count);
                Vector2Int randomCell = emptyCells[number];
                wayponts.Add(new Vector3(randomCell.x, 0, randomCell.y));
                emptyCells.RemoveAt(number);
            }
            player = GameObject.FindWithTag("Player");
            currentState = IdleState;
            currentState.OnEnterState(this);
            audioSource.enabled = false;
            inmortal = false;
            maze.enemies.Add(this.gameObject);
        }
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

    public void SwitchState(Golem_Base_State State)
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
                if(Random.Range(0, 1/RunManager.critProbability) < 1)
                {
                    plus = 1 + RunManager.crit;
                }
                currentHealth.Value -= (Attacker.DañoAct / GameManager.difficulty) * plus;
                DraggableItemUI sword = Attacker.swordEquiped;
                Attacker.ReduceDurabilityClientRpc();
                SwitchState(DamageSate);
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
        yield return new WaitForSeconds(5f);
        maze.enemies.Remove(this.gameObject);
        if(Random.Range(0, 1/RunManager.luck) < 1)
        {
            max = 2;
        }
        for(int i = 0; i < max; i++)
        {
            GameObject droppedItem = Instantiate(itemToDrop, transform.position, Quaternion.identity);
            droppedItem.GetComponent<NetworkObject>().Spawn();
        }
        RunManager.obtainedGold += (int)(this.Health.Value / 20);;
        player.GetComponent<Player>().ShowCoins((int)(this.Health.Value / 20 * (1 + RunManager.goldGain)));
        player.GetComponent<Player>().AddExperience((int)(this.Health.Value / 10 * (1 + RunManager.xpGain)));
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
    public IEnumerator ResetDMG()
    {
        yield return new WaitForSeconds(0.7f);
        inmortal = false;
    }
    public IEnumerator KnowBack()
    {
        // Desactivar el agente para permitir física
        agent.enabled = false;

        // Activar Rigidbody físico
        rb.isKinematic = false;

        // Bloquear rotación para que no se tumbe
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        // Calcular dirección horizontal
        Vector3 dir = (transform.position - player.transform.position).normalized;
        dir.y = 0f;

        // Aplicar fuerza de empuje
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * 100 * Time.deltaTime, ForceMode.Impulse);

        // Esperar duración del empuje
        yield return new WaitForSeconds(0.3f);

        // Parar movimiento físico
        rb.linearVelocity = Vector3.zero;

        // Reactivar el agente y volver al modo kinemático
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        agent.enabled = true;
        SwitchState(PursueState);
    }
    public void ResetAttackCollider()
    {
        attackCollider.enabled = false;

    }
    public void SetupAttackColliders()
    {
        attackCollider.enabled = true;
    }
}
