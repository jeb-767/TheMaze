using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Golem_Dead_State : Golem_Base_State
{
    public override void OnEnterState(Golem_Movement Golem)
    {
        Golem.agent.speed = 0f;
        Golem.anim.SetFloat("Speed", 0f);
        Golem.anim.SetTrigger("Dead");
        Golem.StartCoroutine("Dead");
        Golem.audioSource.enabled = false;
    }
    public override void OnUpdateState(Golem_Movement Golem)
    {

    }
}