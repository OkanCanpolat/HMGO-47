using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection : MonoBehaviour
{
    public Node FirstNode;
    public Node SecondNode;
    public Material LineMaterial;
    public bool isDirty = true;
    private bool isBroken;
    public List<GameObject> Handles = new List<GameObject>();
    public GameObject DoorHandle;
    private List<Vector3> linePoints = new List<Vector3>();
    [SerializeField] private float offsetFromNode = 0.1f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private float highlightedLineWidth;
    public float DoorMarkerWidth = 0.5f;
    public float DoorMarkerOffset = 0.5f;

    [SerializeField] private bool isPotential;
    private ShapeRenderer shapeRenderer;

    public bool IsDirty { get => isDirty; set => isDirty = value; }
    public bool IsBroken
    {
        get
        {
            return isBroken;
        }
        set
        {
            if (value != isBroken)
            {
                isBroken = value;
                isDirty = true;
            }
        }
    }
    public bool IsPotential
    {
        get
        {
            return isPotential;
        }
        set
        {
            if (value != isPotential)
            {
                isPotential = value;
                isDirty = true;
            }
        }
    }
    private void Awake()
    {
        shapeRenderer = FindObjectOfType<ShapeRenderer>();
    }

    public void DrawConnection(float ratio = 1f)
    {
        if (!LineMaterial || !FirstNode || !SecondNode)
        {
            return;
        }

        linePoints.Clear();

        Transform firstNodeTransform = FirstNode.transform;
        Vector3 firstNodePosition = firstNodeTransform.position;
        Vector3 secondOfFirst;
        secondOfFirst = (Handles.Count <= 0) ? SecondNode.transform.position : Handles[0].transform.position;
        Vector3 firstToSecondOfFirst = secondOfFirst - firstNodePosition;
        firstToSecondOfFirst.y = 0f;
        firstToSecondOfFirst.Normalize();
        firstNodePosition += firstToSecondOfFirst * firstNodeTransform.Find("Mesh").transform.localScale.x * offsetFromNode;
        linePoints.Add(firstNodePosition);

        foreach (GameObject handle in Handles)
        {
            linePoints.Add(handle.transform.position);
        }

        Transform secondNodeTransform = SecondNode.transform;
        Vector3 secondNodePosition = secondNodeTransform.position;
        Vector3 secondOfSecond;
        secondOfSecond = (Handles.Count <= 0) ? FirstNode.transform.position : Handles[Handles.Count - 1].transform.position;
        Vector3 SecondToSecondOfSecond = secondOfSecond - secondNodePosition;
        SecondToSecondOfSecond.y = 0f;
        SecondToSecondOfSecond.Normalize();
        secondNodePosition += SecondToSecondOfSecond * secondNodeTransform.Find("Mesh").transform.localScale.x * offsetFromNode;

        linePoints.Add(secondNodePosition);

        AnimPoints(ref linePoints, ratio);
        DrawLine(linePoints, false);
    }
    private void AnimPoints(ref List<Vector3> positions, float ratio)
    {
        if (ratio == 1f || ratio == -1f) return;

        if (ratio < 0f)
        {
            positions.Reverse();
            ratio = 0f - ratio;
        }

        List<Vector3> positionList = new List<Vector3> { positions[0] };

        float currentMagnitude = 0f;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            currentMagnitude += (positions[i] - positions[i + 1]).magnitude;
        }

        float targetMagnitude = currentMagnitude * ratio;
        currentMagnitude = 0f;

        for (int j = 0; j < positions.Count - 1; j++)
        {
            float magnitude = (positions[j] - positions[j + 1]).magnitude;
            currentMagnitude += magnitude;

            if (currentMagnitude < targetMagnitude)
            {
                positionList.Add(positions[j + 1]);
                continue;
            }

            Vector3 direction = (positions[j + 1] - positions[j]).normalized;

            float requestedPartMagnitude = magnitude - (currentMagnitude - targetMagnitude);
            float requestedParRatio = requestedPartMagnitude / magnitude;

            positionList.Add(positions[j] + direction * magnitude * requestedParRatio);

            break;
        }
        positions = positionList;


    }
    private void DrawLine(List<Vector3> positions, bool isHighlighted = false)
    {
        float lineWidth = ((!isHighlighted) ? this.lineWidth : highlightedLineWidth);

        if (IsBroken) return;
       
        if (isPotential)
        {
            if (DoorHandle != null)
            {
                List<Vector3> list = new List<Vector3>();
                List<Vector3> list2 = new List<Vector3>();
                bool flag = true;
                for (int i = 0; i < positions.Count; i++)
                {
                    if (positions[i] == DoorHandle.transform.position)
                    {
                        flag = false;
                    }
                    else if (flag)
                    {
                        list.Add(positions[i]);
                    }
                    else
                    {
                        list2.Add(positions[i]);
                    }
                }
                if (list.Count > 1)
                {
                    shapeRenderer.DrawMultiline(LineMaterial, list, lineWidth);
                }
                if (list2.Count > 1)
                {
                    shapeRenderer.DrawMultiline(LineMaterial, list2, lineWidth);
                }
                if (list2.Count > 0)
                {
                    Vector3 vector = list[list.Count - 1];
                    Vector3 vector2 = list2[0];
                    Vector3 normalized = Vector3.Cross((vector2 - vector).normalized, Vector3.up).normalized;
                    shapeRenderer.DrawLine(LineMaterial, vector - DoorMarkerWidth * normalized, vector + DoorMarkerWidth * normalized, lineWidth);
                    shapeRenderer.DrawLine(LineMaterial, vector2 - DoorMarkerWidth * normalized, vector2 + DoorMarkerWidth * normalized, lineWidth);
                }
            }
            else
            {
                Vector3 vector3 = (linePoints[0] + linePoints[1]) * 0.5f;
                Vector3 vector4 = Vector3.Lerp(linePoints[0], vector3, 1f - DoorMarkerOffset);
                shapeRenderer.DrawLine(LineMaterial, linePoints[0], vector4, lineWidth);
                Vector3 normalized2 = Vector3.Cross((vector4 - linePoints[0]).normalized, Vector3.up).normalized;
                shapeRenderer.DrawLine(LineMaterial, vector4 - DoorMarkerWidth * normalized2, vector4 + DoorMarkerWidth * normalized2, lineWidth);
                Vector3 vector5 = Vector3.Lerp(vector3, linePoints[1], DoorMarkerOffset);
                shapeRenderer.DrawLine(LineMaterial, vector5, linePoints[1], lineWidth);
                Vector3 normalized3 = Vector3.Cross((vector5 - linePoints[0]).normalized, Vector3.up).normalized;
                shapeRenderer.DrawLine(LineMaterial, vector5 - DoorMarkerWidth * normalized3, vector5 + DoorMarkerWidth * normalized3, lineWidth);
            }
        }
        else
        {
            shapeRenderer.DrawMultiline(LineMaterial, positions, lineWidth);
        }
    }

    private void OnDrawGizmos()
    {
        if (!FirstNode || !SecondNode)
        {
            return;
        }
        if (IsOneWay())
        {
            Gizmos.color = Color.magenta;
            DrawOneWayGizmo();
        }
        else if (isPotential)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.black;
        }
        Vector3 from = FirstNode.transform.position;

        foreach (GameObject handle in Handles)
        {
            Vector3 position = handle.transform.position;
            Gizmos.DrawLine(from, position);
            from = position;
        }
        Gizmos.DrawLine(from, SecondNode.transform.position);
    }
    private bool IsOneWay()
    {
        if (!FirstNode || !SecondNode)
        {
            return false;
        }
        if (FirstNode.IsNodeBlocked(SecondNode) || SecondNode.IsNodeBlocked(FirstNode))
        {
            return true;
        }
        return false;
    }
    private void DrawOneWayGizmo()
    {
        if ((bool)FirstNode && (bool)SecondNode)
        {
            Node node;
            Node node2;
            if (!FirstNode.AdjacentNodes.Contains(SecondNode))
            {
                node = SecondNode;
                node2 = FirstNode;
            }
            else
            {
                node = FirstNode;
                node2 = SecondNode;
            }
            Vector3 vector = (node2.transform.position - node.transform.position) / 2f;
            Vector3 vector2 = Vector3.Cross(vector, new Vector3(0f, 1f, 0f)) / 4f;
            Vector3 vector3 = -Vector3.Cross(vector, new Vector3(0f, 1f, 0f)) / 4f;
            Gizmos.DrawLine(node.transform.position + vector, node.transform.position + vector / 2f + vector2);
            Gizmos.DrawLine(node.transform.position + vector, node.transform.position + vector / 2f + vector3);
            Gizmos.DrawLine(node.transform.position + vector + vector2, node.transform.position + vector + vector3);
        }
    }

}

