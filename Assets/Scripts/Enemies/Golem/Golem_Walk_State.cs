using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Golem_Walk_State : Golem_Base_State
{
    RaycastHit hit;
    public override void OnEnterState(Golem_Movement golem)
    {
        golem.audioSource.enabled = true;
        golem.agent.speed = 1.5f;
        golem.agent.SetDestination(golem.wayponts[golem.number]);
    }

    public override void OnUpdateState(Golem_Movement golem)
    {
        Vector3 velocidad = golem.agent.velocity;
        golem.currentSpeed = golem.agent.velocity.magnitude;
        golem.anim.SetFloat("Speed", golem.currentSpeed * 100f, 0.1f, Time.deltaTime);
        golem.anim.SetFloat("Directions", velocidad.z, 0.1f, Time.deltaTime);
        if (!golem.agent.pathPending && golem.agent.remainingDistance <= golem.agent.stoppingDistance)
        {
            if (!golem.agent.hasPath || golem.agent.velocity.sqrMagnitude == 0f)
            {
                if (golem.number == golem.wayponts.Count)
                {
                    golem.number = 0;
                }
                else
                {
                    golem.number++;
                }
                golem.SwitchState(golem.IdleState);
            }
        }
        if (Physics.Raycast(golem.transform.position + Vector3.up * 0.75f, golem.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            golem.player = hit.collider.GetComponentInParent<Player>().gameObject;
            golem.SwitchState(golem.PursueState);
        }
    }
}
