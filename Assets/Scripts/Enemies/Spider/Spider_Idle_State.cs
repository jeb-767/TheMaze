using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spider_Idle_State : Spider_Base_State
{
    private Coroutine idleCoroutine;
    public override void OnEnterState(Spider_Movement Spider)
    {
        Spider.audioSource.enabled = false;
        Spider.anim.SetFloat("Speed" , 0f);
        Spider.agent.speed = 0f;
        idleCoroutine = Spider.StartCoroutine(WaitForIdle(Spider));
    }

    // Update is called once per frame
    public override void OnUpdateState(Spider_Movement Spider)
    {
        
    }
    public IEnumerator WaitForIdle(Spider_Movement Spider)
    {
        yield return new WaitForSeconds(Random.Range(2 , 10));
        Spider.SwitchState(Spider.WalkState);
    }
}
