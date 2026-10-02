using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class Skeleton_Pursue_State : Skeleton_Base_State
{
    public override void OnEnterState(Skeleton_Movement Skeleton)
    {
        Skeleton.agent.speed = 2f;
        Skeleton.audioSource.enabled = true;
        Skeleton.agent.SetDestination(Skeleton.player.transform.position);
    }

    public override void OnUpdateState(Skeleton_Movement Skeleton)
    {
        Skeleton.agent.SetDestination(Skeleton.player.transform.position);
        Vector3 vector = Skeleton.transform.position - Skeleton.player.transform.position; 
        Skeleton.currentSpeed = Skeleton.agent.velocity.magnitude;
        Skeleton.anim.SetFloat("Speed", Skeleton.currentSpeed * 100f); ;
        if (vector.magnitude >= 15f)
        {
            Skeleton.SwitchState(Skeleton.IdleState);
        }
        else if (vector.magnitude <= 1.5f && Skeleton.currentState != Skeleton.AttackState && !Skeleton.isAttacking)
        {
            Skeleton.SwitchState(Skeleton.AttackState);
        }
    }

    public override void OnCollisionEnter(Skeleton_Movement Skeleton)
    {

    }
}
