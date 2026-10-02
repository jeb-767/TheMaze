using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class Skeleton_Dead_State : Skeleton_Base_State

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Skeleton_Movement Skeleton)
    {
        Skeleton.agent.speed = 0f;
        Skeleton.anim.SetFloat("Speed", 0f);
        Skeleton.anim.SetTrigger("Dead");
        Skeleton.StartCoroutine("Dead");
        Skeleton.audioSource.enabled = false;
    }

    // Update is called once per frame
    public override void OnUpdateState(Skeleton_Movement Skeleton)
    {

    }

    public override void OnCollisionEnter(Skeleton_Movement Skeleton)
    {

    }
}
