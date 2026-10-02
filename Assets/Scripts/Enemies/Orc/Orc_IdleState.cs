using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Orc_Idle_State : Orc_Base_State
{
    private Coroutine idleCoroutine;
    public override void OnEnterState(Orc_Movement Orc)
    {
        Orc.audioSource.enabled = false;
        Orc.agent.speed = 0;
        Orc.anim.SetFloat("Speed" , 0f);
        Orc.anim.SetFloat("Idle" , Random.Range(0f , 1f));
        idleCoroutine = Orc.StartCoroutine(WaitForIdle(Orc));
    }
    public override void OnUpdateState(Orc_Movement Orc)
    {
        
    }

    public IEnumerator WaitForIdle(Orc_Movement Orc)
    {
        yield return new WaitForSeconds(Random.Range(2 , 10));
        Orc.SwitchState(Orc.WalkState);
    }
}
