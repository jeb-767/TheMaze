using UnityEngine;

public class ANT_Mum : NPC_Talk
{
    public MisionsBase givedMision;
    public ANT_Child[] child;
    public Canvas_Controller canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        child = FindObjectsOfType<ANT_Child>(true);
        canvas = FindObjectOfType<Canvas_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Interact(Player player)
    {
        Vector3 vector = this.transform.position - child[0].transform.position; 
        if(vector.magnitude <= 2f)
        {
            player.canvas.OpenDialoguePanel("You find my child. Thank's you.");
            MissionManager.Instance.AddProgress(1, 1);
        }
        else
        {
            player.canvas.OpenDialoguePanel("Can you help me to find my child please");
        }
    }
    public override void OnYes()
    {
        player.canvas.CloseDialoguePanel();
        if(child[0].SayYes == false)
        {
            player.canvas.GiveMision(givedMision);
        }
        child[0].SayYes = true;
    }
    public override void OnNo()
    {
        player.canvas.CloseDialoguePanel();
    }
}
