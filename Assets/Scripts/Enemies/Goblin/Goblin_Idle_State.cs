using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

public class Goblin_Idle_State : Goblin_Base_State
{
    private Coroutine idleCoroutine;
    RaycastHit hit;
    public override void OnEnterState(Goblin_Movement Goblin)
    {
        Debug.Log("Idle");
        Goblin.agent.speed = 0;
        Goblin.anim.SetFloat("Speed", 0f);
        Goblin.anim.SetBool("Idle", true);
        idleCoroutine = Goblin.StartCoroutine(WaitForIdle(Goblin));
        Goblin.audioSource.enabled = false;
    }
    public override void OnUpdateState(Goblin_Movement Goblin)
    {
        if (Physics.Raycast(Goblin.transform.position + Vector3.up * 0.75f, Goblin.transform.forward, out hit, 10f) && (hit.collider.CompareTag("Player")))
        {
            Goblin.player = hit.collider.GetComponentInParent<Player>().gameObject;
            if(Goblin.player != null)
            {
                if(Goblin.player.GetComponent<Player>().inventory.items.Count > 0)
                {
                    Goblin.SwitchState(Goblin.PursueState);
                }
            }
        }
    }

    public IEnumerator WaitForIdle(Goblin_Movement Goblin)
    {
        yield return new WaitForSeconds(Random.Range(2, 10));
        Goblin.SwitchState(Goblin.WalkState);
    }
}
