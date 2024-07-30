
public class SniperAiBehavior : AiBehavior
{
    private Pawn targetPawn;

    public SniperAiBehavior(AiPawn pawn)
        : base(pawn)
    {
    }

    public override Pawn EvaluateKillTarget(Node currentNode)
    {
        targetPawn = null;
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        Node nodeInOrientation = pawn.CurrentNode.GetNodeInOrientation(pawn.CurrentOrientation, true);
        while (nodeInOrientation != null)
        {
            if (playerPawn.CurrentNode == nodeInOrientation)
            {
                targetPawn = playerPawn;
                break;
            }
            if (nodeInOrientation.Pawns.Count > 0)
            {
                break;
            }
            nodeInOrientation = nodeInOrientation.GetNodeInOrientation(pawn.CurrentOrientation, true);
        }

        if (targetPawn != playerPawn && pawn.IsEmoting(PawnEmotionType.CanSee))
        {
            pawn.SetEmotion(PawnEmotionType.CantSee);
        }
        return targetPawn;
    }

    public override void ExecuteMove()
    {
        if (pawn.IsEmoting(PawnEmotionType.CanSee) || pawn.IsEmoting(PawnEmotionType.CantSee))
        {
            pawn.UnsetEmotion(false);
        }
        Node closestTargetNode = GetClosestTargetNode();
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if (closestTargetNode != null && closestTargetNode.Pawns.Contains(playerPawn))
        {
            pawn.SetEmotion(PawnEmotionType.CanSee);
        }
        pawn.SetTargetNode(pawn.CurrentNode);
    }

    private Node GetClosestTargetNode()
    {
        Node node = pawn.CurrentNode.GetNodeInOrientation(pawn.CurrentOrientation);
        while (node != null)
        {
            Node nodeInOrientation = node.GetNodeInOrientation(pawn.CurrentOrientation);
            if (node.Pawns.Count > 0 || nodeInOrientation == null)
            {
                break;
            }
            node = nodeInOrientation;
        }
        return node;
    }
}
