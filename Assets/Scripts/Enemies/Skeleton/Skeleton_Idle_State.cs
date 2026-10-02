using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;

public class Skeleton_Idle_State : Skeleton_Base_State
{
    RaycastHit hit;
    private Coroutine idleCoroutine;
    public override void OnEnterState(Skeleton_Movement Skeleton)
    {
        Skeleton.audioSource.enabled = false;
        Skeleton.anim.SetFloat("Speed", 0f);
        Skeleton.agent.speed = 0f;
        if (Physics.Raycast(Skeleton.transform.position + Vector3.up * 0.75f, Skeleton.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            Skeleton.player = hit.collider.GetComponentInParent<Player>().gameObject;
            Skeleton.SwitchState(Skeleton.PursueState);
        }
        else
        {
            idleCoroutine = Skeleton.StartCoroutine(WaitForIdle(Skeleton));
        }
    }

    public override void OnUpdateState(Skeleton_Movement Skeleton)
    {
        if (Physics.Raycast(Skeleton.transform.position + Vector3.up * 0.75f, Skeleton.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            Skeleton.player = hit.collider.gameObject;
            if (idleCoroutine != null)
            {
                Skeleton.StopCoroutine(idleCoroutine);
                idleCoroutine = null;
            }
            Skeleton.SwitchState(Skeleton.PursueState);
        }
    }

    public override void OnCollisionEnter(Skeleton_Movement Skeleton)
    {

    }
    
    public IEnumerator WaitForIdle(Skeleton_Movement Skeleton)
    {
        yield return new WaitForSeconds(Random.Range(2 , 10));
        Skeleton.SwitchState(Skeleton.WalkState);
    }
}
