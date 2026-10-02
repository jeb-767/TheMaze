using UnityEngine;

public class Spider_Damage_State : Spider_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Spider_Movement Spider)
    {
        Spider.anim.SetTrigger("Damaged");
        Spider.anim.SetInteger("DamageNumber", (int)Random.Range(0, 2));
        Spider.inmortal = true;
        Spider.StartCoroutine("KnowBack");
        Spider.StartCoroutine("ResetDMG");
    }

    // Update is called once per frame
    public override void OnUpdateState(Spider_Movement Spider)
    {

    }
}
