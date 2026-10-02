using UnityEngine;

public abstract class Skeleton_Base_State
{
    public abstract void OnEnterState(Skeleton_Movement Skeleton);

    public abstract void OnUpdateState(Skeleton_Movement Skeleton);

    public abstract void OnCollisionEnter(Skeleton_Movement Skeleton);
}
