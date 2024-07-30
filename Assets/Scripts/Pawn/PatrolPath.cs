using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    [SerializeField]
    public List<Node> m_Nodes = new List<Node>();

    [SerializeField]
    public List<Orientation> m_Orientations = new List<Orientation>();

    [SerializeField]
    public bool ShouldContinueIfObstructed = true;

    [HideInInspector]
    private bool pathSelected;

    public List<Node> Nodes => m_Nodes;
    public List<Orientation> Orientations => m_Orientations;
    public bool PathSelected { get => pathSelected; set => pathSelected = value; }
   
    private void OnDrawGizmos()
    {
        if (!pathSelected)
        {
            return;
        }

        float size = 1.2f;

        int nodeCount = m_Nodes.Count;

        for (int i = 0; i < nodeCount; i++)
        {
            float t = i / (float)nodeCount;
            Color color = Color.Lerp(Color.yellow, Color.red, t);
            Gizmos.color = color;
            Node node = m_Nodes[i];

            if (node != null)
            {
                Gizmos.DrawWireCube(node.transform.position, node.transform.localScale * size);
            }
        }
    }
}
