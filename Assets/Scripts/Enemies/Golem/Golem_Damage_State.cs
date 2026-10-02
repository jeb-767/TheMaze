using UnityEngine;

public class Golem_Damage_State : Golem_Base_State
{
    public override void OnEnterState(Golem_Movement Golem)
    {
        Golem.inmortal = true;
        Golem.agent.enabled = false;
        Golem.rb.isKinematic = false;
        Golem.StartCoroutine("KnowBack");
        Golem.StartCoroutine("ResetDMG");
    }

    public override void OnUpdateState(Golem_Movement Golem)
    {
        
    }
}
