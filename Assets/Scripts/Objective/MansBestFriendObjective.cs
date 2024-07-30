using System.Collections.Generic;

public class MansBestFriendObjective : Objective
{
    public override void Awake()
    {
        isMainObjective = false;
        Description = "DON'T KILL DOGS";
        base.Awake();

    }
    public override bool IsComplete()
    {
        List<AiPawn> killedAiPawns = gameManager.KilledAiPawns;

        foreach (AiPawn item in killedAiPawns)
        {
            AiBehavior initialBehavior = item.GetInitialBehavior();
            if (initialBehavior is ChaseDogAiBehavior)
            {
                return false;
            }
        }
        return true;
    }
}
