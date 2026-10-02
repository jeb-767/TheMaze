using UnityEngine;

public class Spider_Walk_State : Spider_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Spider_Movement Spider)
    {
        Spider.audioSource.enabled = true;
        Spider.agent.speed = 1.5f;
        int number = Random.Range(0, Spider.maze.OriginalemptyCells.Count);
        Vector2Int randomCell = Spider.maze.OriginalemptyCells[number];
        Spider.agent.SetDestination(new Vector3(randomCell.x, 0, randomCell.y));    
        //Spider.agent.SetDestination(Spider.wayponts[Random.Range(0 , Spider.wayponts.Count)]);
    }

    // Update is called once per frame
    public override void OnUpdateState(Spider_Movement Spider)
    {
        Spider.currentSpeed = Spider.agent.velocity.magnitude;
        Spider.anim.SetFloat("Speed", Spider.currentSpeed * 100f);
        if (!Spider.agent.pathPending && Spider.agent.remainingDistance <= Spider.agent.stoppingDistance)
        {
            if (!Spider.agent.hasPath || Spider.agent.velocity.sqrMagnitude == 0f)
            {
                Spider.SwitchState(Spider.IdleState);
            }
        }
    }
}
