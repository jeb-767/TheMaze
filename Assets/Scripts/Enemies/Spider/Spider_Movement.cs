using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
public class Spider_Movement : NetworkBehaviour
{

    public Spider_Base_State currentState;
    public Spider_Idle_State IdleState = new Spider_Idle_State();
    public Spider_Walk_State WalkState = new Spider_Walk_State();
    public Spider_Dead_State DeadState = new Spider_Dead_State();
    public Spider_Damage_State DamageSate = new Spider_Damage_State();
    public Spider_Scape_State ScapeState = new Spider_Scape_State();

    public Transform spider;
    public List<Vector3> wayponts = new List<Vector3>();

    public Maze_Generator maze;
    public UnityEngine.AI.NavMeshAgent agent;

    public float currentSpeed;
    public Animator anim;
    public Rigidbody rb;
    public Transform collider;

    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public NetworkVariable<float> Health = new NetworkVariable<float>(30f);
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    public GameObject player;
    public AudioSource audioSource, damageSource, deathSource;
    public bool isDead = false;
    public bool inmortal = false;
    public GameObject itemToDrop;

    /*Método que se ejecuta al principio y situa la rotación de la araña y sus caminos*/
    public override void OnNetworkSpawn()
    {
        if(IsServer) 
        {
            Health.Value = Random.Range(25f, 45f) * GameManager.difficulty;
            currentHealth.Value = Health.Value;
            int control = Random.Range(0, 2);
            maze = GameObject.FindObjectOfType<Maze_Generator>();
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            switch (control)
            {
                case 0:
                    spider.transform.rotation = Quaternion.Euler(0, 0, 180);
                    spider.transform.position = new Vector3(spider.transform.position.x, 1.7f, spider.transform.position.z);
                    break;

                case 1:
                    spider.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }
            currentState = IdleState;
            currentState.OnEnterState(this);
            maze.enemies.Add(this.gameObject);
        }
    }
    [ClientRpc]
    private void PlayDamageSoundClientRpc()
    {
        damageSource.Play();
    }
    /*Método que modifica el estado de la araña en caaa frame*/
    void Update()
    {
        if(!IsServer)
        {
            return;
        }
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

    public void SwitchState(Spider_Base_State State)
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
                collider = other.transform;
                Player Attacker = other.gameObject.GetComponentInParent<Player>();
                player = Attacker.gameObject;
                float plus = 1;
                if(Random.Range(0, 1/RunManager.critProbability) < 1)
                {
                    plus = 1 + RunManager.crit;
                }
                currentHealth.Value -= (Attacker.DañoAct / GameManager.difficulty) * plus;
                Attacker.ReduceDurabilityClientRpc();
                SwitchState(DamageSate);
                damageSource.Play();
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
        RunManager.obtainedGold += (int)(this.Health.Value / 20);
        player.GetComponent<Player>().ShowCoins((int)(this.Health.Value / 20 * (1 + RunManager.goldGain)));
        player.GetComponent<Player>().AddExperience((int)(this.Health.Value / 10 * (1 + RunManager.xpGain)));
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
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
        Vector3 dir = (this.transform.position - collider.position).normalized;
        dir.y = 0f;

        // Aplicar fuerza de empuje
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * Random.Range(125f, 175f) * Time.deltaTime, ForceMode.Impulse);

        // Esperar duración del empuje
        yield return new WaitForSeconds(0.5f);

        // Parar movimiento físico
        rb.linearVelocity = Vector3.zero;

        // Reactivar el agente y volver al modo kinemático
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        agent.enabled = true;
        collider = null;
        SwitchState(ScapeState);
    }
    public IEnumerator ResetDMG()
    {
        yield return new WaitForSeconds(0.7f);
        inmortal = false;
    }
}
