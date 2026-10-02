using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spider_Dead_State : Spider_Base_State
{
    public override void OnEnterState(Spider_Movement Spider)
    {
        Spider.audioSource.enabled = false;
        Spider.agent.speed = 0f;
        Spider.anim.SetFloat("Speed", 0f);
        Spider.anim.SetTrigger("Dead");
        Spider.anim.SetInteger("DeathNumber", (int)Random.Range(0, 2));
        Spider.StartCoroutine("Dead");

    }
    public override void OnUpdateState(Spider_Movement Spider)
    {

    }
}