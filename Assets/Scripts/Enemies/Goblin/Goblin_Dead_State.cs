using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
public class Goblin_Dead_State : Goblin_Base_State
{
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Goblin.agent.speed = 0f;
        Goblin.anim.SetFloat("Speed", 0f);
        Goblin.anim.SetTrigger("Dead");
        Goblin.StartCoroutine("Dead");
        Goblin.audioSource.enabled = false;
    }
    public override void OnUpdateState(Goblin_Movement Goblin)
    {

    }
}
