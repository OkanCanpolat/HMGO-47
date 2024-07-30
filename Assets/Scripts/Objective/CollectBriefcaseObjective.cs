

public class CollectBriefcaseObjective : Objective
{
    public int RequiredNumber = 1;

    public override void Awake()
    {
        IsMainObjective = false;
        Description = "COLLECT BRIEFCASE";
        base.Awake();
    }
    public override bool IsComplete()
    {
        return gameManager.CollectedBriefcases.Count >= RequiredNumber;
    }
}
