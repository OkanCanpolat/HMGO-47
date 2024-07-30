public class StationaryAiBehavior : AiBehavior
{
    public StationaryAiBehavior(AiPawn aiPawn)
        : base(aiPawn)
    {
    }

    public override Node EvaluateNextNode()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;

        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            return playerPawn.CurrentNode;
        }
        return pawn.CurrentNode;
    }
}
