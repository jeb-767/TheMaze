using UnityEngine;

public class Golem_Pursue_State : Golem_Base_State
{
    public override void OnEnterState(Golem_Movement golem)
    {
        golem.agent.speed = 2f;
        golem.audioSource.enabled = true; 
        golem.agent.SetDestination(golem.player.transform.position);
    }

    public override void OnUpdateState(Golem_Movement golem)
    {
        Vector3 velocidad = golem.agent.velocity;
        golem.agent.SetDestination(golem.player.transform.position);
        Vector3 vector = golem.transform.position - golem.player.transform.position;
        golem.currentSpeed = golem.agent.velocity.magnitude;
        golem.anim.SetFloat("Speed", golem.currentSpeed * 100f); ;
        golem.anim.SetFloat("Directions", velocidad.z, 0.1f, Time.deltaTime);
        if (vector.magnitude >= 15f)
        {
            golem.SwitchState(golem.IdleState);
        }
        if (vector.magnitude <= 1.5f && golem.currentState != golem.AttackState)
        {
            golem.SwitchState(golem.AttackState);
        }
    }
}
