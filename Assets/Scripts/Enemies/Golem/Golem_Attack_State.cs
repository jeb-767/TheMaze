using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class Golem_Attack_State : Golem_Base_State
{
    private Coroutine attackCoroutine;
    public override void OnEnterState(Golem_Movement golem)
    {
        golem.audioSource.enabled = false;
        golem.agent.speed = 1f;
        golem.anim.SetTrigger("Attack"); 
        attackCoroutine = golem.StartCoroutine(AttackCooldown(golem));
    }
    // Update is called once per frame
    public override void OnUpdateState(Golem_Movement golem)
    {

    }
    public IEnumerator AttackCooldown(Golem_Movement golem)
    {
        yield return new WaitForSeconds(0.2f);
        golem.SwitchState(golem.PursueState);
    }
}
