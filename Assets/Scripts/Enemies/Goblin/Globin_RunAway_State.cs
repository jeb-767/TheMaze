using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
public class Goblin_RunAway_State : Goblin_Base_State
{
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Debug.Log("RunAway");
        Goblin.agent.speed = 2f;
        Goblin.audioSource.enabled = true;
        Vector2Int randomCell = Goblin.maze.OriginalemptyCells[Random.Range(0, Goblin.maze.OriginalemptyCells.Count)];
        Goblin.agent.SetDestination(new Vector3(randomCell.x, 0, randomCell.y));  
    }

    // Update is called once per frame
    public override void OnUpdateState(Goblin_Movement Goblin)
    {
        Goblin.currentSpeed = Goblin.agent.velocity.magnitude;
        Goblin.anim.SetFloat("Speed" , Goblin.currentSpeed /1.75f , 0.1f , Time.deltaTime);
        if((Goblin.transform.position - Goblin.player.transform.position).magnitude >= 10f)
        {
            Goblin.SwitchState(Goblin.IdleState);
        }
        if(!Goblin.agent.pathPending && Goblin.agent.remainingDistance <= 0.2f)
        {
            Goblin.SwitchState(Goblin.IdleState);
        }
    }
}
