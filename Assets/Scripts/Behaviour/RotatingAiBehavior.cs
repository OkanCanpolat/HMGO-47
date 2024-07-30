
public class RotatingAiBehavior : AiBehavior
{
    public RotationSense RotationSense;

    public int NumberOfQuarterTurns = 2;

    public RotatingAiBehavior(AiPawn pawn, RotationSense rotationSense, int numberOfQuarterTurns)
        : base(pawn)
    {
        RotationSense = rotationSense;
        NumberOfQuarterTurns = numberOfQuarterTurns;
    }

    public override Node EvaluateNextNode()
    {
        Node currentNode = pawn.CurrentNode;
        PlayerPawn playerPawn = gameManager.PlayerPawn;

        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            currentNode = playerPawn.CurrentNode;
        }
        return currentNode;
    }

    public override Orientation EvaluateNextOrientation()
    {
        Orientation orientation = pawn.CurrentOrientation;

        for (int i = 0; i < NumberOfQuarterTurns; i++)
        {
            orientation = orientation.NextOrientation(RotationSense);
        }

        return orientation;
    }
}
