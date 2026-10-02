using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Orc_Walk_State : Orc_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Orc_Movement Orc)
    {
        Orc.audioSource.enabled = true;
        Orc.agent.speed = 1.5f;
        Orc.agent.SetDestination(Orc.wayponts[Orc.number]);
    }

    // Update is called once per frame
    public override void OnUpdateState(Orc_Movement Orc)
    {
        Orc.currentSpeed = Orc.agent.velocity.magnitude;
        Orc.anim.SetFloat("Speed" , Orc.currentSpeed * 100f , 0.1f , Time.deltaTime);
        if (!Orc.agent.pathPending && Orc.agent.remainingDistance <= Orc.agent.stoppingDistance)
        {
            if (!Orc.agent.hasPath || Orc.agent.velocity.sqrMagnitude == 0f)
            {
                if(Orc.number == Orc.wayponts.Count)
                {
                    Orc.number = 0;
                }
                else
                {
                    Orc.number++;
                }
                Orc.SwitchState(Orc.IdleState);
            }
        }
    }
}
