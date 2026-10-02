using UnityEngine;

public class Spider_Scape_State : Spider_Base_State
{
    public override void OnEnterState(Spider_Movement Spider)
    {
        Spider.agent.speed = 2f;
        Spider.audioSource.enabled = true;
        Vector2Int randomCell = Spider.maze.OriginalemptyCells[Random.Range(0, Spider.maze.OriginalemptyCells.Count - 1)];
        Spider.agent.SetDestination(new Vector3(randomCell.x, 0, randomCell.y));  
    }

    // Update is called once per frame
    public override void OnUpdateState(Spider_Movement Spider)
    {
        Spider.currentSpeed = Spider.agent.velocity.magnitude;
        Spider.anim.SetFloat("Speed" , Spider.currentSpeed * 100f , 0.1f , Time.deltaTime);
        if((Spider.transform.position - Spider.player.transform.position).magnitude >= 10f)
        {
            Spider.SwitchState(Spider.IdleState);
        }
        if(Spider.currentSpeed <= 0.1f || Spider.agent.pathPending == false && Spider.agent.remainingDistance <= 0.2f)
        {
            if((Spider.transform.position - Spider.player.transform.position).magnitude >= 10f)
            {
                Spider.SwitchState(Spider.IdleState);
            }
            else
            {
                Spider.SwitchState(Spider.ScapeState);
            }
        }
    }
}
