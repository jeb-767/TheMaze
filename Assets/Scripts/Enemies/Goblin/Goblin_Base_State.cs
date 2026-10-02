using UnityEngine;

public abstract class Goblin_Base_State
{
    public abstract void OnEnterState(Goblin_Movement Goblin);
    public abstract void OnUpdateState(Goblin_Movement Goblin);
}
