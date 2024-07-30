using UnityEngine;
using System.Collections.Generic;

public class ShapeRenderer : MonoBehaviour
{
    private Dictionary<Material, ShapeRendererInstance> instances = new Dictionary<Material, ShapeRendererInstance>();

    private ShapeRendererInstance GetRendererInstance(Material material)
    {
        ShapeRendererInstance rendererInstance;

        if (instances.ContainsKey(material))
        {
            rendererInstance = instances[material];
        }
        else
        {
            GameObject instance = new GameObject();
            instance.transform.parent = transform;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.name = material.name + " renderer";
            rendererInstance = instance.AddComponent<ShapeRendererInstance>();
            rendererInstance.meshFilter = instance.AddComponent<MeshFilter>();
            rendererInstance.meshFilter.mesh = new Mesh();
            MeshRenderer meshRenderer = instance.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            instance.layer = gameObject.layer;
            instances[material] = rendererInstance;
        }
        return rendererInstance;
    }

    public void Clear(Material a_Material)
    {
        GetRendererInstance(a_Material).Clear();
    }

    public void DrawQuadrangle(Material a_Material, Vector3 a_Position0, Vector3 a_Position1, Vector3 a_Position2, Vector3 a_Position3)
    {
        GetRendererInstance(a_Material).DrawQuadrangle(a_Position0, a_Position1, a_Position2, a_Position3);
    }

    public void DrawRectangle(Material a_Material, Vector3 a_StartPosition, Vector3 a_Width, Vector3 a_Length)
    {
        DrawQuadrangle(a_Material, a_StartPosition + a_Width, a_StartPosition + a_Width + a_Length, a_StartPosition, a_StartPosition + a_Length);
    }

    public void DrawLine(Material a_Material, Vector3 a_StartPosition, Vector3 a_EndPosition, float a_Width)
    {
        Vector3 normalized = (a_EndPosition - a_StartPosition).normalized;
        Vector3 vector = Vector3.Cross(Vector3.up, normalized);
        vector.Normalize();
        vector *= a_Width;
        DrawRectangle(a_Material, a_StartPosition - 0.5f * vector, a_EndPosition - a_StartPosition, vector);
    }

    public void DrawMultiline(Material a_Material, List<Vector3> a_Positions, float a_Width)
    {
        _DrawMultiline(a_Material, a_Positions, a_Width, false, 0f, Vector3.up);
    }

    public void DrawMultiline(Material a_Material, List<Vector3> a_Positions, float a_Width, Vector3 a_FacingDirection)
    {
        _DrawMultiline(a_Material, a_Positions, a_Width, false, 0f, a_FacingDirection);
    }

    public void DrawDashedMultiline(Material a_Material, List<Vector3> a_Positions, float a_Width, float a_DashSpacing = 0.5f)
    {
        _DrawMultiline(a_Material, a_Positions, a_Width, true, a_DashSpacing, Vector3.up);
    }

    private void _DrawDashedQuadrangle(Material a_Material, Vector3 a_Position0, Vector3 a_Position1, Vector3 a_Position2, Vector3 a_Position3, float a_DashSpacing)
    {
        bool flag = true;
        float num = Vector3.Distance(0.5f * (a_Position0 + a_Position1), 0.5f * (a_Position2 + a_Position3));
        for (float num2 = 0f; num2 <= num; num2 += a_DashSpacing)
        {
            if (flag)
            {
                Vector3 a_Position4 = Vector3.Lerp(a_Position0, a_Position2, num2 / num);
                Vector3 a_Position5 = Vector3.Lerp(a_Position0, a_Position2, (num2 + a_DashSpacing) / num);
                Vector3 a_Position6 = Vector3.Lerp(a_Position1, a_Position3, num2 / num);
                Vector3 a_Position7 = Vector3.Lerp(a_Position1, a_Position3, (num2 + a_DashSpacing) / num);
                DrawQuadrangle(a_Material, a_Position4, a_Position6, a_Position5, a_Position7);
            }
            flag = !flag;
        }
    }
    private void _DrawMultiline(Material a_Material, List<Vector3> a_Positions, float a_Width, bool a_IsDashed, float a_DashSpacing, Vector3 a_FacingDirection)
    {
        Vector3 a_Position = Vector3.zero;
        Vector3 vector = Vector3.zero;
        int count = a_Positions.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3 vector2 = a_Positions[i];
            Vector3 zero = Vector3.zero;
            if (i == 0)
            {
                zero = Vector3.Cross(a_Positions[1] - a_Positions[0], a_FacingDirection).normalized;
                a_Position = vector2 - 0.5f * zero * a_Width;
                vector = vector2 + 0.5f * zero * a_Width;
                continue;
            }
            if (i == count - 1)
            {
                zero = Vector3.Cross(a_FacingDirection, a_Positions[count - 2] - a_Positions[count - 1]).normalized;
                Vector3 a_Position2 = vector2 - 0.5f * zero * a_Width;
                Vector3 a_Position3 = vector2 + 0.5f * zero * a_Width;
                if (a_IsDashed)
                {
                    _DrawDashedQuadrangle(a_Material, a_Position, vector, a_Position2, a_Position3, a_DashSpacing);
                }
                else
                {
                    DrawQuadrangle(a_Material, a_Position, vector, a_Position2, a_Position3);
                }
                continue;
            }
            Vector3 lhs = (a_Positions[i + 1] - vector2).normalized - (a_Positions[i - 1] - vector2).normalized;
            lhs.Normalize();
            zero = Vector3.Cross(lhs, a_FacingDirection);
            zero.Normalize();
            Vector3 line1Direction = vector2 - a_Positions[i - 1];
            line1Direction.Normalize();
            Vector3 line1Point = vector;
            Vector3 intersectionPosition;
            if (IntersectLines(line1Point, line1Direction, vector2, zero, out intersectionPosition))
            {
                Vector3 vector3 = intersectionPosition;
                Vector3 vector4 = vector2 + (vector2 - intersectionPosition);
                if (a_IsDashed)
                {
                    _DrawDashedQuadrangle(a_Material, a_Position, vector, vector4, vector3, a_DashSpacing);
                }
                else
                {
                    DrawQuadrangle(a_Material, a_Position, vector, vector4, vector3);
                }
                a_Position = vector4;
                vector = vector3;
            }
        }
    }
    private bool IntersectLines(Vector3 line1Point, Vector3 line1Direction, Vector3 line2Point, Vector3 line2Direction, out Vector3 intersectionPosition)
    {
        Vector3 vector = Vector3.Cross(line1Direction, line2Direction);
        if (vector == Vector3.zero)
        {
            intersectionPosition = Vector3.zero;
            return false;
        }
        Vector3 lhs = line2Point - line1Point;
        Vector3 value = Vector3.Cross(lhs, line2Direction);
        Vector3 lhs2 = Vector3.Normalize(vector);
        Vector3 rhs = Vector3.Normalize(value);
        float num = value.magnitude / vector.magnitude;
        float num2 = Vector3.Dot(lhs2, rhs);
        if (num2 == -1f)
        {
            num = 0f - num;
        }
        intersectionPosition = line1Point + line1Direction * num;
        return true;
    }
}
