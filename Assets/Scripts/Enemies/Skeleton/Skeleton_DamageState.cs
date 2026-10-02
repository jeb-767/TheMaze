using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;


public class Skeleton_DamageState : Skeleton_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Skeleton_Movement Skeleton)
    {
        Skeleton.inmortal = true;
        Skeleton.StartCoroutine("KnowBack");
        Skeleton.StartCoroutine("ResetDMG");
    }

    // Update is called once per frame
    public override void OnUpdateState(Skeleton_Movement Skeleton)
    {
        
    }
    public override void OnCollisionEnter(Skeleton_Movement Skeleton)
    {

    }
}
