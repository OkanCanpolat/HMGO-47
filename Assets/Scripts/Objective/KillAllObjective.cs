public class KillAllObjective : Objective
{
    public override void Awake()
    {
        IsMainObjective = false;
        if(Description == string.Empty)
        {
            Description = "KILL ALL ENEMIES";
        }
        base.Awake();
    }

    public override bool IsComplete()
    {
        return gameManager.AiPawns.TrueForAll((AiPawn aiPawn) => aiPawn.IsDead());
    }
}
