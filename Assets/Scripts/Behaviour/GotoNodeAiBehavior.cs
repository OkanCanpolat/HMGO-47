public class GotoNodeAiBehavior : AiBehavior
{
    public delegate void OnArrivedDelegate();

    private Node targetNode;

    private OnArrivedDelegate onArrivedDelegate;

    public GotoNodeAiBehavior(AiPawn pawn, Node targetNode, OnArrivedDelegate onArrivedDelegate)
        : base(pawn)
    {
        this.targetNode = targetNode;
        this.onArrivedDelegate = onArrivedDelegate;
    }

    public override void Activate()
    {
        base.Activate();
        pawn.SetMovingMeshActive(true);
    }

    public override void Deactivate()
    {
        pawn.SetMovingMeshActive(false);
        base.Deactivate();
    }

    public override Node EvaluateNextNode()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            return playerPawn.CurrentNode;
        }
        AStarNodeWrapper start = new AStarNodeWrapper(pawn.CurrentNode, true, pawn.CurrentOrientation);
        AStarNodeWrapper destination = new AStarNodeWrapper(targetNode);
        AStarNodeWrapper.Context context = new AStarNodeWrapper.Context();
        context.m_FaceTwoWays = pawn.InitialBehavior == AiPawn.MovementBehavior.Partners;
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context> aStarSearch = new AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>(start, destination, context);
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Result result = aStarSearch.Resolve();
        if (result.Status == AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Status.Succeeded && result.Path.Length > 1)
        {
            return result.Path[1].Node;
        }
        return pawn.CurrentNode;
    }

    public override Orientation EvaluateNextOrientation()
    {
        Orientation result = pawn.CurrentOrientation;
        Node currentNode = pawn.CurrentNode;
        AStarNodeWrapper start = new AStarNodeWrapper(currentNode);
        AStarNodeWrapper destination = new AStarNodeWrapper(targetNode);
        AStarNodeWrapper.Context context = new AStarNodeWrapper.Context();
        context.m_FaceTwoWays = pawn.InitialBehavior == AiPawn.MovementBehavior.Partners;
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context> aStarSearch = new AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>(start, destination, context);
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Result result2 = aStarSearch.Resolve();
        if (result2.Status == AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Status.Succeeded && result2.Path.Length > 1)
        {
            Node node = result2.Path[1].Node;
            result = currentNode.GetOrientationToNode(node);
        }
        return result;
    }

    public override void ExecuteMove()
    {
        pawn.SetTargetNode(EvaluateNextNode());
    }

    public override void OnArrived(Node node)
    {
        if (node == targetNode)
        {
            onArrivedDelegate();
        }
    }
}
