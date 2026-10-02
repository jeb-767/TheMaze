using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using Unity.Netcode;
public class Skeleton_Movement : NetworkBehaviour
{
    public Skeleton_Base_State currentState;
    public Skeleton_Idle_State IdleState = new Skeleton_Idle_State();
    public Skeleton_Pursue_State PursueState = new Skeleton_Pursue_State();
    public Skeleton_Walk_Around_State WalkState = new Skeleton_Walk_Around_State();
    public Skeleton_Attack_State AttackState = new Skeleton_Attack_State();
    public Skeleton_Dead_State DeadState = new Skeleton_Dead_State();
    public Skeleton_DamageState DamageSate = new Skeleton_DamageState();


    public UnityEngine.AI.NavMeshAgent agent;

    public Maze_Generator maze;

    public float currentSpeed;
    public GameObject player;
    public Animator anim;
    public NetworkVariable<float> Health = new NetworkVariable<float>(500f);
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    public AudioSource audioSource, damageSource, deadSource, attackSource;
    public bool inmortal = false;
    [SerializeField] public Rigidbody rb;
    public BoxCollider attackCollider1, attackCollider2;
    private bool isDead = false;
    public bool isAttacking = false;
    public GameObject itemToDrop;
    void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Health.Value = Random.Range(25f, 45f) * GameManager.difficulty;
            maze = GameObject.FindObjectOfType<Maze_Generator>();
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            //player = GameObject.FindWithTag("Player");
            currentState = IdleState;
            currentState.OnEnterState(this);
            currentHealth.Value = Health.Value;
            audioSource.enabled = false;
            attackCollider1.enabled = false;
            attackCollider2.enabled = false;
            maze.enemies.Add(this.gameObject);
            SwitchState(IdleState);
        }
    }
    void Update()
    {
        if(!IsServer)
        {
            return;
        }
        /*if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }*/
        if (currentHealth.Value <= 0f && !isDead)
        {
            isDead = true;
            SwitchState(DeadState);
            deadSource.Play();
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
    }

    public void SwitchState(Skeleton_Base_State State)
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
                Player attacker = other.GetComponentInParent<Player>();
                player = attacker.gameObject;
                float plus = 1;
                if(Random.Range(0, 1/RunManager.critProbability) < 1)
                {
                    plus = 1 + RunManager.crit;
                }
                currentHealth.Value -= (attacker.DañoAct / GameManager.difficulty) * plus;
                attacker.ReduceDurabilityClientRpc();
                SwitchState(DamageSate);
                anim.SetTrigger("Damaged");
                PlayDamageSoundClientRpc();
            }
            else
            {
                return;
            }
        }
        
    }
    [ClientRpc]
    private void PlayDamageSoundClientRpc()
    {
        damageSource.Play();
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
        rb.AddForce(dir * Random.Range(200f, 250f) * Time.deltaTime, ForceMode.Impulse);

        // Esperar duración del empuje
        yield return new WaitForSeconds(0.5f);

        // Parar movimiento físico
        rb.linearVelocity = Vector3.zero;

        // Reactivar el agente y volver al modo kinemático
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        agent.enabled = true;
        SwitchState(PursueState);
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
        RunManager.obtainedGold += (int)(this.Health.Value / 20);
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
    public void ResetAttackCollider()
    {
        attackCollider1.enabled = false;
        attackCollider2.enabled = false;

    }
    public void SetupAttackColliders()
    {
        attackCollider1.enabled = true;
        attackCollider2.enabled = true;
    }
}
