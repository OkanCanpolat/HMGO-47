using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Node : MonoBehaviour
{
    public float ManyPawnsRadius = 0.5f;

    public Transform MeshTransform;

    public List<Node> AdjacentNodes = new List<Node>();

    public List<Node> BlockedNodes = new List<Node>();

    public List<Connection> ConnectionsData = new List<Connection>();

    public List<Node> PotentialConnections = new List<Node>();

    public List<Connection> PotentialConnectionsData = new List<Connection>();

    public Material TriangleMaterial;

    public GameObject SwipeIndicatorPrefab;

    public GameObject ClickIndicatorPrefab;

    public GameObject HatchIndicatorPrefab;

    public CameraPosition CameraPosition;

    public AudioClip Music;

    private List<Pawn> pawns = new List<Pawn>();

    private List<Pawn> arrivingPawns = new List<Pawn>();

    private List<Pawn> departingPawns = new List<Pawn>();

    private NodeAttribute[] cachedNodeAttributes;

    private NodeItem cachedNodeItem;

    private ObjectPool objectPool;

    private Dictionary<AiPawn, Vector3> targetPositions = new Dictionary<AiPawn, Vector3>();

    private Vector3 playerTargetPosition;

    private Vector3 nodeItemTargetPosition;

    private bool isSwipable;

    private bool isClickable;

    private GameObject indicator;

    private int indicatorLayer;

    [Inject] private GameManager gameManager;
    [Inject] private NodeManager nodeManager;
    public List<Pawn> Pawns => pawns;
    public List<Pawn> ArrivingPawns => arrivingPawns;
    public List<Pawn> DepartingPawns => departingPawns;
    public NodeAttribute[] CachedNodeAttributes => cachedNodeAttributes;
    public NodeItem CachedNodeItem
    {
        get
        {
            if ((bool)cachedNodeItem && !cachedNodeItem.gameObject.activeInHierarchy)
            {
                return null;
            }
            return cachedNodeItem;
        }
    }
    public bool IsClickable => isClickable;
    public bool IsSwipable => isSwipable;

    private void Start()
    {
        objectPool = gameManager.GetComponent<ObjectPool>();
        MeshTransform = transform.Find("Mesh");
        cachedNodeAttributes = GetComponentsInChildren<NodeAttribute>();
        cachedNodeItem = GetComponentInChildren<NodeItem>();

        if (TriangleMaterial != null)
        {
            Orientation oneWayOrientation = GetOneWayOrientation();

            if (oneWayOrientation != 0)
            {
                MeshTransform.GetComponent<MeshRenderer>().material = TriangleMaterial;
                MeshTransform.rotation = oneWayOrientation.OrientationToQuaternion();
            }
        }
    }
    public Orientation GetOneWayOrientation()
    {
        for (int i = 0; i < AdjacentNodes.Count; i++)
        {
            if (AdjacentNodes[i].IsNodeBlocked(this))
            {
                return GetOrientationToNode(AdjacentNodes[i]);
            }
        }
        return Orientation.Invalid;
    }
    public Connection GetConnectionData(Node node)
    {
        int num = AdjacentNodes.IndexOf(node);
        return (num < 0 || num >= ConnectionsData.Count) ? null : ConnectionsData[num];
    }
    public void RemoveAllConnections()
    {
        for (int num = AdjacentNodes.Count - 1; num >= 0; num--)
        {
            AdjacentNodes[num].RemoveConnection(this);
            RemoveConnection(AdjacentNodes[num]);
        }
    }
    public void RemoveConnection(Node otherNode)
    {
        int num = AdjacentNodes.IndexOf(otherNode);
        if (num != -1)
        {
            ConnectionsData[num].IsBroken = true;
            AdjacentNodes.RemoveAt(num);
            ConnectionsData.RemoveAt(num);
        }
    }
    public Vector3 GetPositionForPawn(Pawn pawn)
    {
        PlayerPawn playerPawn = pawn as PlayerPawn;
        if (playerPawn != null)
        {
            return playerTargetPosition;
        }
        AiPawn key = pawn as AiPawn;
        return targetPositions[key];
    }
    public void OnPlayerInteract(Node selectedNode, Barrier barrier)
    {
        NodeAttribute[] cachedNodeAttributes = CachedNodeAttributes;

        foreach (NodeAttribute nodeAttribute in cachedNodeAttributes)
        {
            nodeAttribute.OnPlayerInteract(selectedNode, barrier);
        }
    }
    public void OnPlayerStartInteract(Barrier barrier)
    {
        NodeAttribute[] cachedNodeAttributes = CachedNodeAttributes;
        foreach (NodeAttribute nodeAttribute in cachedNodeAttributes)
        {
            nodeAttribute.OnPlayerStartInteract(barrier);
        }
    }
    public NodeAttributeType[] GetNodeAttributes<NodeAttributeType>() where NodeAttributeType : NodeAttribute
    {
        return transform.GetComponentsInChildren<NodeAttributeType>();
    }

    public NodeAttributeType GetNodeAttribute<NodeAttributeType>() where NodeAttributeType : NodeAttribute
    {
        return transform.GetComponentInChildren<NodeAttributeType>();
    }
    public void DisplayIndicator(bool displayClickIndicator)
    {
        if (displayClickIndicator)
        {
            isClickable = true;
            if (ClickIndicatorPrefab != null)
            {
                indicator = objectPool.GetObjectForType(ClickIndicatorPrefab.name, false);
            }
        }

        else
        {
            isSwipable = true;

            if (SwipeIndicatorPrefab != null)
            {
                indicator = objectPool.GetObjectForType(SwipeIndicatorPrefab.name, false);
            }
        }
        SetParentIndicator();
    }
    public void DisplayHatchIndicator()
    {
        isClickable = true;

        if (HatchIndicatorPrefab != null)
        {
            indicator = objectPool.GetObjectForType(HatchIndicatorPrefab.name, false);
        }

        SetParentIndicator();
    }
    public void OnPawnArrival(Pawn pawn, Barrier barrier)
    {
        NodeAttribute[] cachedNodeAttributes = CachedNodeAttributes;
        foreach (NodeAttribute nodeAttribute in cachedNodeAttributes)
        {
            nodeAttribute.OnPawnArrival(pawn, barrier);
        }
    }
    public void OnPlayerPawnKilled()
    {
        NodeAttribute[] componentsInChildren = GetComponentsInChildren<NodeAttribute>();

        foreach (NodeAttribute nodeAttribute in componentsInChildren)
        {
            nodeAttribute.OnPlayerPawnKilled();
        }
    }
    public void OnPawnDeparture(Pawn pawn, Barrier barrier)
    {
        NodeAttribute[] cachedNodeAttributes = CachedNodeAttributes;
        foreach (NodeAttribute nodeAttribute in cachedNodeAttributes)
        {
            nodeAttribute.OnPawnDeparture(pawn, barrier);
        }
    }
    private void SetParentIndicator()
    {
        if ((bool)indicator)
        {
            indicator.transform.parent = transform;

            IndicatorAnimation component = indicator.GetComponent<IndicatorAnimation>();

            if ((bool)component)
            {
                component.StartAnimation();
            }
        }
    }

    public Vector3 GetPositionForNodeItem()
    {
        return nodeItemTargetPosition;
    }
    public void ComputeOptimalPawnsTargetPositions()
    {
        targetPositions.Clear();

        NodeItem cachedNodeItem = CachedNodeItem;
        Vector3 position = transform.position;

        int pawnCount = Pawns.Count;
        int arrivingPawnCount = ArrivingPawns.Count;
        int totalPawnCount = pawnCount + arrivingPawnCount;

        if (cachedNodeItem != null && !Pawns.Contains(gameManager.PlayerPawn) && !ArrivingPawns.Contains(gameManager.PlayerPawn))
        {
            totalPawnCount++;
        }

        switch (totalPawnCount)
        {
            case 0:
                return;
            case 1:
                if (pawnCount == 1)
                {
                    AiPawn aiPawn = Pawns[0] as AiPawn;

                    if (aiPawn != null)
                    {
                        targetPositions[aiPawn] = position;
                    }
                    else
                    {
                        playerTargetPosition = position;
                    }
                }
                else if (arrivingPawnCount == 1)
                {
                    AiPawn aiPawn = ArrivingPawns[0] as AiPawn;

                    if (aiPawn != null)
                    {
                        targetPositions[aiPawn] = position;
                    }
                    else
                    {
                        playerTargetPosition = position;
                    }
                }
                if (cachedNodeItem != null)
                {
                    nodeItemTargetPosition = position;
                }
                return;
        }

        Permutator permutator = new Permutator();
        List<Vector3> list = new List<Vector3>();
        List<Vector3> list2 = new List<Vector3>();

        foreach (Pawn pawn in Pawns)
        {
            if (pawn as PlayerPawn != null && cachedNodeItem != null)
            {
                list.Add(cachedNodeItem.transform.position);
            }
            else
            {
                list.Add(pawn.transform.position);
            }
        }
        foreach (Pawn arrivingPawn in ArrivingPawns)
        {
            if (arrivingPawn as PlayerPawn != null && cachedNodeItem != null)
            {
                list.Add(cachedNodeItem.transform.position);
            }
            else
            {
                list.Add(arrivingPawn.transform.position);
            }
        }
        if (cachedNodeItem != null && !Pawns.Contains(gameManager.PlayerPawn) && !ArrivingPawns.Contains(gameManager.PlayerPawn))
        {
            list.Add(cachedNodeItem.transform.position);
        }

        for (int i = 0; i < totalPawnCount; i++)
        {
            float f =  (float)i / (float)totalPawnCount * 2f * Mathf.PI;
            Vector3 item = position + ManyPawnsRadius * (Vector3.forward * Mathf.Cos(f) + Vector3.left * Mathf.Sin(f));
            list2.Add(item);
        }

        int[] results = permutator.Compute(list, list2);

        int index = 0;

        foreach (Pawn pawn in Pawns)
        {
            PlayerPawn playerPawn = pawn as PlayerPawn;

            if (playerPawn != null)
            {
                playerTargetPosition = list2[results[index]];

                if (cachedNodeItem != null)
                {
                    nodeItemTargetPosition = playerTargetPosition;
                }
            }
            else
            {
                AiPawn key = pawn as AiPawn;
                targetPositions[key] = list2[results[index]];
            }
            index++;
        }
        foreach (Pawn arrivingPawn in ArrivingPawns)
        {
            PlayerPawn palyerPawn = arrivingPawn as PlayerPawn;

            if (palyerPawn != null)
            {
                playerTargetPosition = list2[results[index]];
                if (cachedNodeItem != null)
                {
                    nodeItemTargetPosition = playerTargetPosition;
                }
            }
            else
            {
                AiPawn key = arrivingPawn as AiPawn;
                targetPositions[key] = list2[results[index]];
            }
            index++;
        }
        if (cachedNodeItem != null && !Pawns.Contains(gameManager.PlayerPawn) && !ArrivingPawns.Contains(gameManager.PlayerPawn))
        {
            nodeItemTargetPosition = list2[results[index]];
        }
    }
    public int GetManhattanDistance(Node otherNode)
    {
        Vector3 position = otherNode.transform.position;
        Vector3 position2 = transform.position;
        float distance = nodeManager.Distance;

        int num = (int)(Mathf.Abs(position.x - position2.x) / distance);
        int num2 = (int)(Mathf.Abs(position.z - position2.z) / distance);
        return num + num2;
    }
    public void OnEnterNodeSelection(bool shouldSelectNodeForInteraction)
    {
        if (!shouldSelectNodeForInteraction)
        {
            foreach (Node adjacentNode in AdjacentNodes)
            {
                if (!IsNodeBlocked(adjacentNode))
                {
                    adjacentNode.DisplayIndicator(false);
                }
            }
        }

        NodeAttribute[] componentsInChildren = GetComponentsInChildren<NodeAttribute>();
        NodeAttribute[] temp = componentsInChildren;

        foreach (NodeAttribute nodeAttribute in temp)
        {
            nodeAttribute.OnEnterNodeSelection(shouldSelectNodeForInteraction);
        }
    }
    public void AddConnection(Node otherNode)
    {
        int num = AdjacentNodes.IndexOf(otherNode);
        int num2 = PotentialConnections.IndexOf(otherNode);

        if (num == -1 && num2 >= 0)
        {
            PotentialConnectionsData[num2].IsPotential = false;
            AdjacentNodes.Add(otherNode);
            ConnectionsData.Add(PotentialConnectionsData[num2]);
            PotentialConnections.RemoveAt(num2);
            PotentialConnectionsData.RemoveAt(num2);
        }
    }

    public bool IsNodeBlocked(Node node)
    {
        return BlockedNodes.Contains(node);
    }
    public void OnLeaveNodeSelection()
    {
        NodeAttribute[] componentsInChildren = GetComponentsInChildren<NodeAttribute>();
        NodeAttribute[] temp = componentsInChildren;

        foreach (NodeAttribute nodeAttribute in temp)
        {
            nodeAttribute.OnLeaveNodeSelection();
        }

        if (indicator != null)
        {
            objectPool.PoolObject(indicator.gameObject);
            indicator = null;
        }
        isSwipable = false;
        isClickable = false;
    }
    public bool IsInCubeBoxDistanceFrom(Node centerNode, float cubeHalfSize)
    {
        Vector3 currentPos = transform.position;
        Vector3 nodePos = centerNode.transform.position;

        if (Mathf.Abs(currentPos.x - nodePos.x) <= cubeHalfSize && Mathf.Abs(currentPos.z - nodePos.z) <= cubeHalfSize)
        {
            return true;
        }

        return false;
    }

    public bool IsInCrossDistanceFrom(Node centerNode, float crossHalfSize)
    {
        Vector3 position = transform.position;
        Vector3 position2 = centerNode.transform.position;
        if (Mathf.Abs(position.x - position2.x) <= crossHalfSize && Mathf.Abs(position.z - position2.z) <= Mathf.Epsilon)
        {
            return true;
        }
        if (Mathf.Abs(position.x - position2.x) <= Mathf.Epsilon && Mathf.Abs(position.z - position2.z) <= crossHalfSize)
        {
            return true;
        }

        return false;
    }

    public bool IsInDiamondDistanceFrom(Node centerNode, float diamondHalfSize)
    {
        Vector3 position = transform.position;
        Vector3 position2 = centerNode.transform.position;
        float num = Mathf.Abs(position.x - position2.x) + Mathf.Abs(position.z - position2.z);
        return num < diamondHalfSize;
    }
    public Node GetNodeInOrientation(Orientation a_Orientation, bool ignoreBlockedNodes = false)
    {
        foreach (Node adjacentNode in AdjacentNodes)
        {
            if (ignoreBlockedNodes || !IsNodeBlocked(adjacentNode))
            {
                Orientation orientation = OrientationEnumMethods.OrientationFromTwoPositions(transform.position, adjacentNode.transform.position);
                
                if (a_Orientation == orientation)
                {
                    return adjacentNode;
                }
            }
        }
        return null;
    }
    public bool IsConnectedTo(Node node)
    {
        return AdjacentNodes.Contains(node);
    }
    public void GetAllNodesInOrientation(Orientation a_Orientation, int distance, ref List<Node> allNodesInOrientation, bool ignoreBlockedNodes = false)
    {
        if (distance <= 0)
        {
            return;
        }
        foreach (Node adjacentNode in AdjacentNodes)
        {
            if (ignoreBlockedNodes || !IsNodeBlocked(adjacentNode))
            {
                Orientation orientation = OrientationEnumMethods.OrientationFromTwoPositions(base.transform.position, adjacentNode.transform.position);
                
                if (a_Orientation == orientation)
                {
                    allNodesInOrientation.Add(adjacentNode);
                    adjacentNode.GetAllNodesInOrientation(a_Orientation, distance - 1, ref allNodesInOrientation);
                }
            }
        }
    }
    public Orientation GetOrientationToNode(Node toNode, bool ignoreBlockedNodes = false)
    {
        int num = AdjacentNodes.IndexOf(toNode);

        if (num == -1 || (!ignoreBlockedNodes && IsNodeBlocked(toNode)))
        {
            return Orientation.Invalid;
        }

        return OrientationEnumMethods.OrientationFromTwoPositions(transform.position, toNode.transform.position);
    }
    public void OnPlayerSniperShot(Barrier barrier)
    {
        NodeAttribute[] cachedNodeAttributes = CachedNodeAttributes;
        foreach (NodeAttribute nodeAttribute in cachedNodeAttributes)
        {
            nodeAttribute.OnPlayerSniperShot(barrier);
        }
    }
}
