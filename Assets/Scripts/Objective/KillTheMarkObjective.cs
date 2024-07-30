
public class KillTheMarkObjective : Objective
{
    private AiPawn pawn;

    public override void Awake()
    {
        isMainObjective = true;

        if (Description == string.Empty)
        {
            Description = "KILL YOUR MARK";
        }
        base.Awake();
    }

    public void SetTarget(AiPawn pawn)
    {
        this.pawn = pawn;
    }

    public override bool IsComplete()
    {
        return pawn != null && pawn.IsDead();
    }

}
