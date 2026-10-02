using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
public class Skeleton_Walk_Around_State : Skeleton_Base_State
{
    RaycastHit hit;
    int x = 0;
    int z = 0;
    Vector2Int randomCell;
    public override void OnEnterState(Skeleton_Movement Skeleton)
    {
        Skeleton.audioSource.enabled = true;
        Skeleton.agent.speed = 1.5f;
        int number = Random.Range(0, Skeleton.maze.OriginalemptyCells.Count);
        Vector2Int randomCell = Skeleton.maze.OriginalemptyCells[number];
        Skeleton.agent.SetDestination(new Vector3(randomCell.x, 0, randomCell.y));    
    }

    public override void OnUpdateState(Skeleton_Movement Skeleton)
    {
        Skeleton.currentSpeed = Skeleton.agent.velocity.magnitude;
        Skeleton.anim.SetFloat("Speed", Skeleton.currentSpeed * 100f);
        Vector3 vector = new Vector3(Skeleton.transform.position.x - randomCell.x, 0, Skeleton.transform.position.z - randomCell.y);
        if (!Skeleton.agent.pathPending && Skeleton.agent.remainingDistance <= Skeleton.agent.stoppingDistance)
        {
            if (!Skeleton.agent.hasPath || Skeleton.agent.velocity.sqrMagnitude == 0f)
            {
                Skeleton.SwitchState(Skeleton.IdleState);
            }
        }
        if (Physics.Raycast(Skeleton.transform.position + Vector3.up * 0.75f, Skeleton.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            Skeleton.player = hit.collider.GetComponentInParent<Player>().gameObject;
            Skeleton.SwitchState(Skeleton.PursueState);
        }
    }

    public override void OnCollisionEnter(Skeleton_Movement Skeleton)
    {

    }
}

