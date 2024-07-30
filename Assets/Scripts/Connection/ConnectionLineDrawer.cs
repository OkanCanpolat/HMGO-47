using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[RequireComponent (typeof(ShapeRenderer))]
public class ConnectionLineDrawer : MonoBehaviour
{
    public EnvironmentSoundConfig EnvironmentSoundConfig;
    [Inject] private NodeManager nodeManager;
    [Inject] private GameManager gameManager;
    [Inject] private AudioManager audioManager;
    private bool inConnectionAnim;

    private List<Node> animNodeList;

    private List<Node> animCloseNodeList;

    private float connectionAnimStartTime;

    public float connectionsAnimDuration = 0.2f;

    private CameraSphericalMovement cameraSphericalMovement;
    public bool InConnectionAnim => inConnectionAnim;

    private void Start()
    {
        cameraSphericalMovement = Camera.main.GetComponent<CameraSphericalMovement>();
        animNodeList = new List<Node>();
        animCloseNodeList = new List<Node>();
        inConnectionAnim = true;
        connectionAnimStartTime = 0f;
        animNodeList.Add(gameManager.PlayerPawn.CurrentNode);
    }
    private void Update()
    {
        if (inConnectionAnim)
        {
            ShowConnectionsAnimation();
        }

        else
        {
            ShowConnections();
        }
    }
    private void ShowConnections()
    {
        List<Connection> connections = nodeManager.Connections;
        bool flag = false;

        foreach (Connection connection in connections)
        {
            if (connection.IsDirty)
            {
                flag = true;
                break;
            }
        }

        if (flag)
        {
            foreach (Connection connection2 in connections)
            {
                connection2.DrawConnection();
                connection2.IsDirty = false;
            }
        }
    }

    private void ShowConnectionsAnimation()
    {
        if (cameraSphericalMovement != null && !cameraSphericalMovement.IsInAnimation())
        {
            if (Time.timeSinceLevelLoad == 0f) return;

            if (connectionAnimStartTime == 0f)
            {
                connectionAnimStartTime = Time.timeSinceLevelLoad;
                EnableRenderer(nodeManager.Connections, false, true);
                audioManager.PlaySoundOnce(EnvironmentSoundConfig.NodeGraphRevealStartSound, EnvironmentSoundConfig.NodeGraphRevealStartVolume);
            }

            if (connectionAnimStartTime + connectionsAnimDuration > Time.timeSinceLevelLoad)
            {
                float ratio = (Time.timeSinceLevelLoad - connectionAnimStartTime) / connectionsAnimDuration;
                List<Connection> cachedConnections = nodeManager.Connections;

                foreach (Connection connection in cachedConnections)
                {
                    if (!connection.IsDirty)
                    {
                        connection.DrawConnection();
                    }
                }
                {
                    foreach (Node animNode in animNodeList)
                    {
                        DrawConnection(animNode.ConnectionsData, ratio);
                        DrawConnection(animNode.PotentialConnectionsData, ratio);
                    }
                    return;
                }
            }
            connectionAnimStartTime = Time.timeSinceLevelLoad;
            animCloseNodeList.AddRange(animNodeList);
            foreach (Node animNode2 in animNodeList)
            {
                EnableRenderer(animNode2.ConnectionsData, true, false);
                EnableRenderer(animNode2.PotentialConnectionsData, true, false);
            }
            List<Node> list = new List<Node>();
            list.AddRange(animNodeList);
            animNodeList.Clear();
            foreach (Node item in list)
            {
                foreach (Connection connectionsDatum in item.ConnectionsData)
                {
                    AddUniqueNode(animNodeList, connectionsDatum.FirstNode, animCloseNodeList);
                    AddUniqueNode(animNodeList, connectionsDatum.SecondNode, animCloseNodeList);
                }
                foreach (Connection potentialConnectionsDatum in item.PotentialConnectionsData)
                {
                    AddUniqueNode(animNodeList, potentialConnectionsDatum.FirstNode, animCloseNodeList);
                    AddUniqueNode(animNodeList, potentialConnectionsDatum.SecondNode, animCloseNodeList);
                }

                SecretPassageNodeAttribute componentInChildren = item.GetComponentInChildren<SecretPassageNodeAttribute>();
                if ((bool)componentInChildren && componentInChildren.TargetNode != null)
                {
                    AddUniqueNode(animNodeList, componentInChildren.TargetNode, animCloseNodeList);
                }
            }
            if (animNodeList.Count == 0)
            {
                EnableRenderer(nodeManager.Connections, true, true);
                inConnectionAnim = false;
            }
        }
        else
        {
            EnableRenderer(nodeManager.Connections, false, false);
        }
    }
    private void DrawConnection(List<Connection> connectionsData, float ratio)
    {
        foreach (Connection connection in connectionsData)
        {
            if (connection.IsDirty)
            {
                if (animNodeList.Contains(connection.FirstNode))
                {
                    connection.DrawConnection(ratio);
                }
                else
                {
                    connection.DrawConnection(0f - ratio);
                }
            }
        }
    }

    private void AddUniqueNode(List<Node> aNodeList, Node aNode, List<Node> aExcludeNodeList)
    {
        if (aExcludeNodeList == null || !aExcludeNodeList.Contains(aNode))
        {
            if (!aNodeList.Contains(aNode)) aNodeList.Add(aNode);
        }
    }
    private void EnableRenderer(List<Connection> aConnectionsData, bool aEnable, bool aDirty)
    {
        foreach (Connection aConnectionsDatum in aConnectionsData)
        {
            aConnectionsDatum.IsDirty = aDirty;
            if ((bool)aConnectionsDatum.FirstNode)
            {
                EnableRenderer(aConnectionsDatum.FirstNode, aEnable);
            }
            if ((bool)aConnectionsDatum.SecondNode)
            {
                EnableRenderer(aConnectionsDatum.SecondNode, aEnable);
            }
        }
    }
    private void EnableRenderer(Node node, bool enable)
    {
        node.MeshTransform.GetComponent<Renderer>().enabled = enable;
        Renderer[] componentsInChildren = node.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if ((bool)componentsInChildren[i].GetComponent<SwipeIndicatorAnimation>() || (bool)componentsInChildren[i].GetComponent<ExtractionPointAnimation>())
            {
                componentsInChildren[i].enabled = enable;
            }
        }
    }

    //private IEnumerator DrawConnections()
    //{
    //    bool loop = true;

    //    while (loop)
    //    {
    //        float t = 0;

    //        while (t < 1)
    //        {

    //            //List<MConnection> cachedConnections = nodeManager.Connections;
    //            //foreach (MConnection connection in cachedConnections)
    //            //{
    //            //    if (!connection.IsDirty)
    //            //    {
    //            //        connection.DrawConnectionLine();
    //            //    }
    //            //}

    //            foreach (Node animNode in animNodes)
    //            {
    //                DrawConnection(animNode.ConnectionsData, t);
    //                DrawConnection(animNode.PotentialConnectionsData, t);
    //            }

    //            t += Time.deltaTime / animTime;
    //            yield return null;
    //        }

    //        foreach (Node animNode in animNodes)
    //        {
    //            DrawConnection(animNode.ConnectionsData, 1);
    //            DrawConnection(animNode.PotentialConnectionsData, 1);
    //        }


    //        animCompletedNodes.AddRange(animNodes);

    //        foreach (Node node in animNodes)
    //        {
    //            EnableRenderer(node.ConnectionsData, true, false);
    //            EnableRenderer(node.PotentialConnectionsData, true, false);
    //        }

    //        List<Node> animNodesTemp = new List<Node>();
    //        animNodesTemp.AddRange(animNodes);
    //        animNodes.Clear();

    //        foreach (Node node in animNodesTemp)
    //        {
    //            foreach (Connection connection in node.ConnectionsData)
    //            {
    //                if (!animCompletedNodes.Contains(connection.FirstNode)) animNodes.Add(connection.FirstNode);
    //                if (!animCompletedNodes.Contains(connection.SecondNode)) animNodes.Add(connection.SecondNode);
    //            }

    //            foreach (Connection connection in node.PotentialConnectionsData)
    //            {
    //                if (!animCompletedNodes.Contains(connection.FirstNode)) animNodes.Add(connection.FirstNode);
    //                if (!animCompletedNodes.Contains(connection.SecondNode)) animNodes.Add(connection.SecondNode);
    //            }
    //        }

    //        if (animNodes.Count <= 0) loop = false;
    //    }
    //}
    //private LineRenderer GetLineRenderer(Connection connection)
    //{
    //    LineRenderer lineRenderer;

    //    if (connectionLineMap.TryGetValue(connection, out lineRenderer))
    //    {
    //        return lineRenderer;
    //    }

    //    lineRenderer = Instantiate(rendererToolPrefab, transform);
    //    lineRenderer.startWidth = lineWidth;
    //    lineRenderer.endWidth = lineWidth;
    //    lineRenderer.material = lineMaterial;
    //    connectionLineMap.Add(connection, lineRenderer);
    //    return lineRenderer;
    //}
    //private void DrawConnection(List<Connection> connections, float ratio)
    //{
    //    foreach (Connection connection in connections)
    //    {
    //        Vector3 firstPoint = connection.FirstNode.transform.position;
    //        Vector3 firstToSecond = connection.SecondNode.transform.position - connection.FirstNode.transform.position;
    //        firstToSecond.y = 0;
    //        firstToSecond.Normalize();
    //        firstPoint += firstToSecond * connection.FirstNode.MeshTransform.localScale.x * offsetFromNode;

    //        Vector3 secondPoint = connection.SecondNode.transform.position;
    //        Vector3 secondToFirst = connection.FirstNode.transform.position - connection.SecondNode.transform.position;
    //        secondToFirst.y = 0;
    //        secondToFirst.Normalize();
    //        secondPoint += secondToFirst * connection.SecondNode.MeshTransform.localScale.x * offsetFromNode;

    //        Vector3 point = Vector3.Lerp(firstPoint, secondPoint, ratio);

    //        LineRenderer lineRenderer = GetLineRenderer(connection);
    //        lineRenderer.positionCount++;
    //        lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);

    //        //if (connection.IsDirty)
    //        //{
    //        //    if (animNodes.Contains(connection.FirstNode))
    //        //    {
    //        //        connection.DrawConnectionLine(ratio);
    //        //    }
    //        //    else
    //        //    {
    //        //        connection.DrawConnectionLine(0f - ratio);
    //        //    }
    //        //}
    //    }
    //}
    //private void EnableRenderer(List<Connection> connections, bool enable, bool dirty)
    //{
    //    foreach (Connection connection in connections)
    //    {
    //        connection.IsDirty = dirty;

    //        if ((bool)connection.FirstNode)
    //        {
    //            EnableRenderer(connection.FirstNode, enable);
    //        }
    //        if ((bool)connection.SecondNode)
    //        {
    //            EnableRenderer(connection.SecondNode, enable);
    //        }
    //    }
    //}
    //private void EnableRenderer(Node node, bool enable)
    //{
    //    node.MeshTransform.GetComponent<Renderer>().enabled = enable;
    //    //Renderer[] componentsInChildren = aNode.GetComponentsInChildren<Renderer>();
    //    //for (int i = 0; i < componentsInChildren.Length; i++)
    //    //{
    //    //    if ((bool)componentsInChildren[i].GetComponent<SwipeIndicatorAnimation>() || (bool)componentsInChildren[i].GetComponent<ExtractionPointAnimation>())
    //    //    {
    //    //        componentsInChildren[i].enabled = aEnable;
    //    //    }
    //    //}
    //}
}
