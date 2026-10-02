using UnityEngine;

public class Orc_Scape_State : Orc_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Orc_Movement Orc)
    {
        Orc.agent.speed = 2f;
        Orc.audioSource.enabled = true;
        Vector2Int randomCell = Orc.maze.OriginalemptyCells[Random.Range(0, Orc.maze.OriginalemptyCells.Count - 1)];
        Orc.agent.enabled = true;
        Orc.agent.SetDestination(new Vector3(randomCell.x, 0, randomCell.y));  
    }

    // Update is called once per frame
    public override void OnUpdateState(Orc_Movement Orc)
    {
        Orc.currentSpeed = Orc.agent.velocity.magnitude;
        Orc.anim.SetFloat("Speed" , Orc.currentSpeed * 100f , 0.1f , Time.deltaTime);
        if((Orc.transform.position - Orc.player.transform.position).magnitude >= 10f)
        {
            Orc.SwitchState(Orc.IdleState);
        }
        if(Orc.currentSpeed <= 0.1f || Orc.agent.pathPending == false && Orc.agent.remainingDistance <= 0.2f)
        {
            if((Orc.transform.position - Orc.player.transform.position).magnitude >= 10f)
            {
                Orc.SwitchState(Orc.IdleState);
            }
            else
            {
                Orc.SwitchState(Orc.ScapeState);
            }
        }
    }
}
