
public class StrafingPartnersAiBehavior : AiBehavior
{
    public StrafingPartnersAiBehavior(AiPawn pawn)
        : base(pawn)
    {
    }

    public override Node EvaluateNextNode()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            return playerPawn.CurrentNode;
        }
        Node nodeInOrientation = pawn.CurrentNode.GetNodeInOrientation(pawn.CurrentOrientation.NextOrientation(RotationSense.ClockWise));
        if (nodeInOrientation != null && nodeInOrientation.Pawns.Contains(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            return playerPawn.CurrentNode;
        }
        Orientation currentOrientation = pawn.CurrentOrientation;
        Node currentNode = pawn.CurrentNode;
        Node nodeInOrientation2 = currentNode.GetNodeInOrientation(currentOrientation);
        if (nodeInOrientation2 != null)
        {
            return nodeInOrientation2;
        }
        return currentNode;
    }

    public override Orientation EvaluateNextOrientation()
    {
        Orientation orientation = pawn.CurrentOrientation;
        Node currentNode = pawn.CurrentNode;
        Node nodeInOrientation = currentNode.GetNodeInOrientation(orientation);
        if (nodeInOrientation == null)
        {
            orientation = orientation.OppositeOrientation();
        }
        return orientation;
    }
}
