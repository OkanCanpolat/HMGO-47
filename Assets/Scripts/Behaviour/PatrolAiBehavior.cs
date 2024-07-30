using UnityEngine;

public class PatrolAiBehavior : AiBehavior
{
    private enum PathingDirection
    {
        Forward,
        Backward
    }

    public PatrolPath Path;

    private int m_CurrentNodeIndex = -1;

    private PathingDirection currentPathingDirection;

    public PatrolAiBehavior(AiPawn pawn, PatrolPath path)
        : base(pawn)
    {
        Path = path;
        currentPathingDirection = PathingDirection.Forward;
    }

    public override Node EvaluateNextNode()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            return playerPawn.CurrentNode;
        }
        int num = Path.m_Nodes.IndexOf(pawn.CurrentNode);
        if (num != -1)
        {
            int num2 = FindNextConnectedNodeIndex(num);
            if (num2 != -1)
            {
                return Path.m_Nodes[num2];
            }
        }
        else
        {
            Node nearestNodeOnPath = GetNearestNodeOnPath(pawn.CurrentNode);
            AStarNodeWrapper start = new AStarNodeWrapper(pawn.CurrentNode, true, pawn.CurrentOrientation);
            AStarNodeWrapper destination = new AStarNodeWrapper(nearestNodeOnPath);
            AStarNodeWrapper.Context context = new AStarNodeWrapper.Context();
            context.m_FaceTwoWays = pawn.InitialBehavior == AiPawn.MovementBehavior.Partners;
            AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context> aStarSearch = new AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>(start, destination, context);
            AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Result result = aStarSearch.Resolve();
            if (result.Status == AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Status.Succeeded && result.Path.Length > 1)
            {
                return result.Path[1].Node;
            }
        }
        return pawn.CurrentNode;
    }

    public override void ExecuteMove()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            pawn.SetTargetNode(playerPawn.CurrentNode);
            return;
        }
        m_CurrentNodeIndex = Path.m_Nodes.IndexOf(pawn.CurrentNode);
        if (m_CurrentNodeIndex != -1)
        {
            if (pawn.IsEmoting(PawnEmotionType.ReturnToPatrol))
            {
                pawn.UnsetEmotion(false);
            }
            int num = FindNextConnectedNodeIndex(m_CurrentNodeIndex);
            if (num != -1)
            {
                Node targetNode = Path.m_Nodes[num];
                pawn.SetTargetNode(targetNode);
                bool flag = (currentPathingDirection == PathingDirection.Forward && num < m_CurrentNodeIndex) || (currentPathingDirection == PathingDirection.Backward && num > m_CurrentNodeIndex);
                int num2 = FindNextConnectedNodeIndex(num);
                m_CurrentNodeIndex = num;
                bool flag2 = Path.m_Nodes[0] == Path.Nodes[Path.m_Nodes.Count - 1];
                if (flag || num2 == -1 || (!flag2 && currentPathingDirection == PathingDirection.Forward && m_CurrentNodeIndex == Path.m_Nodes.Count - 1) || (currentPathingDirection == PathingDirection.Backward && m_CurrentNodeIndex == 0))
                {
                    currentPathingDirection = ((currentPathingDirection == PathingDirection.Forward) ? PathingDirection.Backward : PathingDirection.Forward);
                }
            }
            else
            {
                currentPathingDirection = ((currentPathingDirection == PathingDirection.Forward) ? PathingDirection.Backward : PathingDirection.Forward);
            }
            return;
        }
        Node nearestNodeOnPath = GetNearestNodeOnPath(pawn.CurrentNode);
        AStarNodeWrapper start = new AStarNodeWrapper(pawn.CurrentNode, true, pawn.CurrentOrientation);
        AStarNodeWrapper destination = new AStarNodeWrapper(nearestNodeOnPath);
        AStarNodeWrapper.Context context = new AStarNodeWrapper.Context();
        context.m_FaceTwoWays = pawn.InitialBehavior == AiPawn.MovementBehavior.Partners;
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context> aStarSearch = new AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>(start, destination, context);
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Result result = aStarSearch.Resolve();
        if (result.Status == AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Status.Succeeded && result.Path.Length > 1)
        {
            if (!pawn.IsEmoting(PawnEmotionType.ReturnToPatrol))
            {
                pawn.SetEmotion(PawnEmotionType.ReturnToPatrol);
            }
            Node node = result.Path[1].Node;
            pawn.SetTargetNode(node);
            if (node == nearestNodeOnPath)
            {
                m_CurrentNodeIndex = Path.m_Nodes.IndexOf(node);
            }
        }
        else
        {
            pawn.SetTargetNode(pawn.CurrentNode);
        }
    }

    public override void ExecuteRotation()
    {
        m_CurrentNodeIndex = Path.m_Nodes.IndexOf(pawn.CurrentNode);
        if (m_CurrentNodeIndex != -1)
        {
            if (pawn.IsEmoting(PawnEmotionType.ReturnToPatrol))
            {
                pawn.UnsetEmotion(false);
            }
            Node node = Path.m_Nodes[m_CurrentNodeIndex];
            int num = FindNextConnectedNodeIndex(m_CurrentNodeIndex);
            Orientation orientation = ((currentPathingDirection != 0) ? Path.m_Orientations[m_CurrentNodeIndex].OppositeOrientation() : Path.m_Orientations[m_CurrentNodeIndex]);
            if (num == -1)
            {
                currentPathingDirection = ((currentPathingDirection == PathingDirection.Forward) ? PathingDirection.Backward : PathingDirection.Forward);
                orientation = orientation.OppositeOrientation();
            }
            else if (num != -1 && node.GetNodeInOrientation(pawn.CurrentOrientation) != Path.m_Nodes[num])
            {
                orientation = node.GetOrientationToNode(Path.m_Nodes[num]);
            }
            pawn.SetTargetOrientation(orientation);
            return;
        }
        Node currentNode = pawn.CurrentNode;
        Node nearestNodeOnPath = GetNearestNodeOnPath(pawn.CurrentNode);
        AStarNodeWrapper start = new AStarNodeWrapper(currentNode);
        AStarNodeWrapper destination = new AStarNodeWrapper(nearestNodeOnPath);
        AStarNodeWrapper.Context context = new AStarNodeWrapper.Context();
        context.m_FaceTwoWays = pawn.InitialBehavior == AiPawn.MovementBehavior.Partners;
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context> aStarSearch = new AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>(start, destination, context);
        AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Result result = aStarSearch.Resolve();
        if (result.Status == AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.Status.Succeeded && result.Path.Length > 1)
        {
            if (!pawn.IsEmoting(PawnEmotionType.ReturnToPatrol))
            {
                pawn.SetEmotion(PawnEmotionType.ReturnToPatrol);
            }
            Node node2 = result.Path[1].Node;
            Orientation orientationToNode = currentNode.GetOrientationToNode(node2);
            pawn.SetTargetOrientation(orientationToNode);
        }
    }

    private Node GetNearestNodeOnPath(Node fromNode)
    {
        int num = int.MaxValue;
        Node result = Path.m_Nodes[0];
        for (int i = 0; i < Path.m_Nodes.Count; i++)
        {
            if (Path.m_Nodes[i] != null)
            {
                int manhattanDistance = fromNode.GetManhattanDistance(Path.m_Nodes[i]);
                if (manhattanDistance < num)
                {
                    num = manhattanDistance;
                    result = Path.m_Nodes[i];
                }
            }
        }
        return result;
    }

    private int GetNextNodeIndex(int currentNodeIndex)
    {
        bool flag = Path.m_Nodes[0] == Path.Nodes[Path.m_Nodes.Count - 1];
        int num = ((currentPathingDirection != 0) ? (currentNodeIndex - 1) : (currentNodeIndex + 1));
        if (currentPathingDirection == PathingDirection.Forward)
        {
            if (num >= Path.m_Nodes.Count)
            {
                num = (flag ? 1 : (Path.m_Nodes.Count - 2));
            }
        }
        else if (currentPathingDirection == PathingDirection.Backward && num < 0)
        {
            num = ((!flag) ? 1 : (Path.m_Nodes.Count - 2));
        }
        return num;
    }

    private int FindNextConnectedNodeIndex(int currentNodeIndex)
    {
        Node node = Path.m_Nodes[currentNodeIndex];
        int num = 0;
        int num2 = currentNodeIndex;
        do
        {
            num2 = GetNextNodeIndex(num2);
            Node node2 = Path.m_Nodes[num2];
            if (node2.IsConnectedTo(node) || node2 == node)
            {
                return num2;
            }
            num++;
        }
        while (num <= Path.m_Nodes.Count);
        return -1;
    }
}
