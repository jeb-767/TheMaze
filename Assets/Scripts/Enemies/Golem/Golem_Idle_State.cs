using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Golem_Idle_State : Golem_Base_State
{
    private Coroutine idleCoroutine;
    RaycastHit hit;
    public override void OnEnterState(Golem_Movement golem)
    {
        golem.agent.speed = 0;
        golem.anim.SetFloat("Speed", 0f);
        golem.anim.SetFloat("Idle", Random.Range(0f, 1f));
        idleCoroutine = golem.StartCoroutine(WaitForIdle(golem));
        golem.audioSource.enabled = false;
    }
    public override void OnUpdateState(Golem_Movement golem)
    {
        if (Physics.Raycast(golem.transform.position + Vector3.up * 0.75f, golem.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            golem.player = hit.collider.GetComponentInParent<Player>().gameObject;
            if (idleCoroutine != null)
            {
                golem.StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }
            golem.SwitchState(golem.PursueState);
        }
    }

    public IEnumerator WaitForIdle(Golem_Movement golem)
    {
        yield return new WaitForSeconds(Random.Range(2, 10));
        golem.SwitchState(golem.WalkState);
    }
}
