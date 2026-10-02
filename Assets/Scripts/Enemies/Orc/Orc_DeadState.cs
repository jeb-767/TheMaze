using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Orc_Dead_State : Orc_Base_State
{
    public override void OnEnterState(Orc_Movement Orc)
    {
        Orc.audioSource.enabled = false;
        Orc.agent.speed = 0f;
        Orc.anim.SetFloat("Speed", 0f);
        Orc.anim.SetTrigger("Dead");
        Orc.StartCoroutine("Dead");
    }
    public override void OnUpdateState(Orc_Movement Orc)
    {

    }
}