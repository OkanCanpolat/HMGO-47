using System.Collections.Generic;

public class DoNotKillObjective : Objective
{
    public override void Awake()
    {
        IsMainObjective = false;

        if (Description == string.Empty) Description = "NO KILL";
        base.Awake();
    }
    public override bool IsComplete()
    {
        List<AiPawn> killedAiPawns = gameManager.KilledAiPawns;

        foreach (AiPawn item in killedAiPawns)
        {
            if (item.gameObject.GetComponent<TargetPawnObjective>() == null)
            {
                return false;
            }
        }
        return true;
    }
}
