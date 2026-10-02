using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class Skeleton_Attack_State : Skeleton_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Coroutine attackCoroutine;
    public override void OnEnterState(Skeleton_Movement Skeleton)
    {
        Skeleton.agent.speed = 1f;
        Skeleton.audioSource.enabled = false;
        Skeleton.anim.SetTrigger("LeftAttack");
        attackCoroutine = Skeleton.StartCoroutine(AttackCooldown(Skeleton));
    }
    // Update is called once per frame
    public override void OnUpdateState(Skeleton_Movement Skeleton)
    {
    }
    public override void OnCollisionEnter(Skeleton_Movement Skeleton)
    {

    }
    public IEnumerator AttackCooldown(Skeleton_Movement Skeleton)
    {
        Skeleton.isAttacking = true;
        yield return new WaitForSeconds(0.5f);
        Skeleton.SwitchState(Skeleton.PursueState);
        Skeleton.isAttacking = false;
    }
}
