using UnityEngine;

public class Goblin_Pursue_State : Goblin_Base_State
{
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Goblin.agent.speed = 2f;
        Goblin.audioSource.enabled = true; 
        Goblin.agent.SetDestination(Goblin.player.transform.position);
    }

    public override void OnUpdateState(Goblin_Movement Goblin)
    {
        Vector3 velocidad = Goblin.agent.velocity;
        Goblin.agent.SetDestination(Goblin.player.transform.position);
        Vector3 vector = Goblin.transform.position - Goblin.player.transform.position;
        Goblin.currentSpeed = Goblin.agent.velocity.magnitude;
        Goblin.anim.SetFloat("Speed", Goblin.currentSpeed / 1.75f); ;
        if (vector.magnitude >= 10f)
        {
            Goblin.SwitchState(Goblin.IdleState);
        }
        if (vector.magnitude <= 1f && Goblin.currentState != Goblin.AttackState)
        {
            Goblin.SwitchState(Goblin.AttackState);
        }
    }
}
