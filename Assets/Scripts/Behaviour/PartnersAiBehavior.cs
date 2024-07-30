public class PartnersAiBehavior : AiBehavior
{
    public PartnersAiBehavior(AiPawn pawn)
         : base(pawn)
    {
    }

    public override Node EvaluateNextNode()
    {
        Node result = pawn.CurrentNode;
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        Node currentNode = playerPawn.CurrentNode;

        if (pawn.IsPawnValidTarget(playerPawn))
        {
            Orientation orientationToNode = pawn.CurrentNode.GetOrientationToNode(currentNode);
            Orientation currentOrientation = pawn.CurrentOrientation;

            if (orientationToNode == currentOrientation || orientationToNode == currentOrientation.OppositeOrientation())
            {
                result = currentNode;
            }
        }
        return result;
    }
}
