using UnityEngine;
using UnityEngine.AI;

public class ANT_Child : NPC_Talk
{
    public NavMeshAgent agent;
    public bool SayYes;
    public Animator anim;
    public MisionsBase givedMision;
    public Canvas_Controller canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = FindObjectOfType<Canvas_Controller>();
        SayYes = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(SayYes)
        {
            agent.SetDestination(player.transform.position);
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }
            if(agent.velocity.magnitude > 0.1f)
            {
                anim.SetBool("Walking", true);
            }
            else
            {
                anim.SetBool("Walking", false);
            }
        }
    }
    public override void Interact(Player player)
    {
        // Ejecuta la lógica base (guardar al jugador, reproducir sonido, etc.)
        base.Interact(player);
        Debug.Log("Interacting with ANT_Child");
        // Abre el panel de diálogo con el texto de bienvenida usando el método que ya tienes
        player.canvas.OpenDialoguePanel("Hello! I am a child ant. Help me to find my mom");
    }
    public override void OnYes()
    {
        // 1. Cerramos el panel de diálogo normal
        player.canvas.CloseDialoguePanel();
        if(SayYes == false)
        {
            player.canvas.GiveMision(givedMision);
        }
        SayYes = true;
        // 2. Le decimos al Canvas del jugador que abra el menú de misiones.
        // (Tendrás que crear este método OpenQuestPanel() en el script de tu Canvas)
        //player.canvas.OpenQuestPanel(); 
    }
    public override void OnNo()
    {
        // Si el jugador dice que no, simplemente cerramos la conversación
        player.canvas.CloseDialoguePanel();
    }
}
