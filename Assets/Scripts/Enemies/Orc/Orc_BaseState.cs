using UnityEngine;

public abstract class Orc_Base_State
{
    public abstract void OnEnterState(Orc_Movement Orc);
    public abstract void OnUpdateState(Orc_Movement Orc);
}

