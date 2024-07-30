using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class NodeManager : MonoBehaviour
{
    public List<Connection> Connections;

    [SerializeField] private List<Node> nodes = new List<Node>();

    [SerializeField] private float distance = 4f;
    public List<Node> Nodes => nodes;
    public float Distance { get => distance; set => distance = value; }

    private void Awake()
    {
        Init();
    }

    public void RemoveNode(Node node)
    {
        if (node != null)
        {
            node.RemoveAllConnections();
            Nodes.Remove(node);
            Destroy(node.gameObject);
        }
    }
    public void Init()
    {
        Connections = FindObjectsOfType<Connection>().ToList();
        nodes = GetComponentsInChildren<Node>().ToList();
    }
    public void BindNodeAndConnections()
    {
        foreach (Node node in nodes)
        {
            node.ConnectionsData.Clear();
            node.PotentialConnections.Clear();
            node.PotentialConnectionsData.Clear();

            foreach (Connection connection in Connections)
            {
                if (connection.FirstNode == node || connection.SecondNode == node)
                {
                    if (connection.IsPotential)
                    {
                        node.PotentialConnectionsData.Add(connection);
                        Node potentialNode = connection.FirstNode == node ? connection.SecondNode : connection.FirstNode;
                        node.PotentialConnections.Add(potentialNode);
                    }

                    else
                    {
                        node.ConnectionsData.Add(connection);
                    }
                }
            }
        }
    }
    public void BindAdjacentNodes()
    {
        foreach (Node node in nodes)
        {
            node.AdjacentNodes.Clear();
            List<Connection> connections = node.ConnectionsData;

            foreach (Connection connection in connections)
            {
                if (connection.FirstNode != node) node.AdjacentNodes.Add(connection.FirstNode);
                if (connection.SecondNode != node) node.AdjacentNodes.Add(connection.SecondNode);
            }
        }
    }
    public void ParseConnection()
    {
        bool fisrNode = true;

        foreach (Connection connection in Connections)
        {
            fisrNode = true;
            string conName = connection.name;

            for (int i = 0; i < conName.Length; i++)
            {
                if (char.IsDigit(conName[i]))
                {
                    string numberStr = conName[i].ToString();
                    int foundNumber = FindAllDigits(conName, i + 1, ref numberStr);
                    i += foundNumber;
                    int number = int.Parse(numberStr);
                    Debug.Log(numberStr);


                    foreach (Node node in nodes)
                    {
                        string NodeNameNumber = "";
                        FindAllDigits(node.name, 0, ref NodeNameNumber, true);
                        if (NodeNameNumber == string.Empty) return;

                        if (NodeNameNumber == numberStr)
                        {
                            if (fisrNode)
                            {
                                connection.FirstNode = node;
                                fisrNode = false;
                                break;
                            }
                            else
                            {
                                connection.SecondNode = node;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    [ContextMenu("SetupAll")]
    public void SetupAll()
    {
        Init();
        ParseConnection();
        BindNodeAndConnections();
        BindAdjacentNodes();
    }
    private int FindAllDigits(string source, int index, ref string result, bool lookAll = false)
    {
        int returnValue = 0;

        for (int i = index; i < source.Length; i++)
        {
            if (char.IsDigit(source[i]))
            {
                returnValue++;
                result += source[i];
            }

            else
            {
                if (lookAll) continue;
                return returnValue;
            }
        }

        return returnValue;

    }
}
