using UnityEngine;

public abstract class Spider_Base_State
{
    public abstract void OnEnterState(Spider_Movement Spider);

    public abstract void OnUpdateState(Spider_Movement Spider);
}
