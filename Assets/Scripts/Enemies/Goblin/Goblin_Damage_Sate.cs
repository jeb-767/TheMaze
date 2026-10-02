using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

public class Goblin_Damage_State : Goblin_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Goblin.inmortal = true;
        Goblin.agent.enabled = false;
        Goblin.rb.isKinematic = false;
        Goblin.anim.SetTrigger("Damaged");
        Goblin.StartCoroutine("KnowBack");
        Goblin.StartCoroutine("ResetDMG");
    }

    public override void OnUpdateState(Goblin_Movement Goblin)
    {
        
    }
}
