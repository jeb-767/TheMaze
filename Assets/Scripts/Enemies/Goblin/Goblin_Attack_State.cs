using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

public class Goblin_Attack_State : Goblin_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Coroutine attackCoroutine;
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Debug.Log("Attack");
        Goblin.audioSource.enabled = false;
        Goblin.agent.speed = 1f;
        Goblin.anim.SetTrigger("Attack"); 
        attackCoroutine = Goblin.StartCoroutine(AttackCooldown(Goblin));
    }
    // Update is called once per frame
    public override void OnUpdateState(Goblin_Movement Goblin)
    {

    }
    public IEnumerator AttackCooldown(Goblin_Movement Goblin)
    {
        yield return new WaitForSeconds(0.5f);
        if (Goblin.currentState != Goblin.RunAwayState) 
        {
            Goblin.SwitchState(Goblin.PursueState);
        }
    }
}
