using System.Collections.Generic;

public class AStarNodeWrapper : AStarSearch<AStarNodeWrapper, AStarNodeWrapper.Context>.ISearchNode
{
    public class Context
    {
        public bool m_FaceTwoWays;
    }

    public Node Node;

    public bool FirstNode;

    public Orientation FirstNodeOrientation;

    public AStarNodeWrapper(Node node)
    {
        Node = node;
    }

    public AStarNodeWrapper(Node node, bool firstNode, Orientation firstNodeOrientation)
    {
        Node = node;
        FirstNode = firstNode;
        FirstNodeOrientation = firstNodeOrientation;
    }

    public bool IsSameNodeThan(AStarNodeWrapper otherNode, Context context)
    {
        if (otherNode != null)
        {
            return Node == otherNode.Node && FirstNode == otherNode.FirstNode;
        }
        return false;
    }

    public IEnumerable<AStarSearch<AStarNodeWrapper, Context>.Connection> GetConnections(Context context)
    {
        bool faceTwoWays = context.m_FaceTwoWays;
        if (!FirstNode)
        {
            foreach (Node nodeAroundCurrentNode in Node.AdjacentNodes)
            {
                if (!Node.BlockedNodes.Contains(nodeAroundCurrentNode))
                {
                    AStarNodeWrapper nodeWrapper = new AStarNodeWrapper(nodeAroundCurrentNode);
                    yield return new AStarSearch<AStarNodeWrapper, Context>.Connection(nodeWrapper, 1f);
                }
            }
            yield break;
        }
        AStarNodeWrapper currentNodeWrapper = new AStarNodeWrapper(Node);
        yield return new AStarSearch<AStarNodeWrapper, Context>.Connection(currentNodeWrapper, 1f);
        Node facedNode = Node.GetNodeInOrientation(FirstNodeOrientation);
        if (facedNode != null)
        {
            AStarNodeWrapper facedNodeWrapper = new AStarNodeWrapper(facedNode);
            yield return new AStarSearch<AStarNodeWrapper, Context>.Connection(facedNodeWrapper, 1f);
        }
        if (faceTwoWays)
        {
            Node oppositeNode = Node.GetNodeInOrientation(FirstNodeOrientation.OppositeOrientation());
            if (oppositeNode != null)
            {
                AStarNodeWrapper oppositeNodeWrapper = new AStarNodeWrapper(oppositeNode);
                yield return new AStarSearch<AStarNodeWrapper, Context>.Connection(oppositeNodeWrapper, 1f);
            }
        }
    }

    public float EstimateCostToDestination(AStarNodeWrapper destination, Context context)
    {
        return 1f;
    }
}
