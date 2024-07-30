
using UnityEngine;

public class TargetPawnObjective : Objective
{
    public override void Awake()
    {
        isMainObjective = true;
        Description = "KILL YOUR MARK";
        base.Awake();
    }
    public override bool IsComplete()
    {
        AiPawn component = gameObject.GetComponent<AiPawn>();

        if ((bool)component && component.IsDead())
        {
            return true;
        }

        return false;
    }
}
