using UnityEngine;

public abstract class Golem_Base_State
{
    public abstract void OnEnterState(Golem_Movement golem);
    public abstract void OnUpdateState(Golem_Movement golem);
}
