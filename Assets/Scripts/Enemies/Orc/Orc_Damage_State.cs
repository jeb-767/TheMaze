using UnityEngine;

public class Orc_Damage_State : Orc_Base_State
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEnterState(Orc_Movement Orc)
    {
        Orc.anim.SetTrigger("Damaged");
        Orc.inmortal = true;
        Orc.StartCoroutine("KnowBack");
        Orc.StartCoroutine("ResetDMG");
    }

    // Update is called once per frame
    public override void OnUpdateState(Orc_Movement Orc)
    {
        
    }
}
