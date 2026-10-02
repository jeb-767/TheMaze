using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
public class Goblin_Walk_State : Goblin_Base_State
{
    RaycastHit hit;
    int x = 0; 
    int z = 0;
    Vector2Int randomCell;
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Debug.Log("Walk");
        Goblin.audioSource.enabled = true;
        Goblin.agent.speed = 1.5f;
        int number = Random.Range(0, Goblin.maze.OriginalemptyCells.Count);
        randomCell = Goblin.maze.OriginalemptyCells[number];
        Goblin.agent.SetDestination(new Vector3(randomCell.x, 0, randomCell.y)); 
        Goblin.anim.SetBool("Idle", false);
    }

    public override void OnUpdateState(Goblin_Movement Goblin)
    {
        Goblin.currentSpeed = Goblin.agent.velocity.magnitude;
        Goblin.anim.SetFloat("Speed", Goblin.currentSpeed /1.75f);
        Vector3 vector = new Vector3(Goblin.transform.position.x - randomCell.x, 0, Goblin.transform.position.z - randomCell.y);
        if (!Goblin.agent.pathPending && Goblin.agent.remainingDistance <= Goblin.agent.stoppingDistance)
        {
            if (!Goblin.agent.hasPath || Goblin.agent.velocity.sqrMagnitude == 0f)
            {
                Goblin.SwitchState(Goblin.IdleState);
            }
        }
        if (Physics.Raycast(Goblin.transform.position + Vector3.up * 0.75f, Goblin.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            Goblin.player = hit.collider.GetComponentInParent<Player>().gameObject;
            if(Goblin.player != null)
            {
                if(Goblin.player.GetComponent<Player>().inventory.items.Count > 0)
                {
                    Goblin.SwitchState(Goblin.PursueState);
                }
            }
        }
    }
}
