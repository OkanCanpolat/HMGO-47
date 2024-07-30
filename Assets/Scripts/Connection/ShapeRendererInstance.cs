using System.Collections.Generic;
using UnityEngine;

public class ShapeRendererInstance : MonoBehaviour
{
    public List<Vector3> vertices = new List<Vector3>();

    public List<Vector2> uvs = new List<Vector2>();

    public List<int> triangles = new List<int>();

    public MeshFilter meshFilter;

    private bool isDirty = true;

    private void Update()
    {
        if (isDirty)
        {
            meshFilter.mesh.Clear();
            meshFilter.mesh.vertices = vertices.ToArray();
            meshFilter.mesh.uv = uvs.ToArray();
            meshFilter.mesh.triangles = triangles.ToArray();
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
            isDirty = false;
        }
    }
    public void DrawQuadrangle(Vector3 position0, Vector3 position1, Vector3 position2, Vector3 position3)
    {
        isDirty = true;
        int count = vertices.Count;
        vertices.Add(position0);
        uvs.Add(new Vector2(0f, 0f));
        vertices.Add(position1);
        uvs.Add(new Vector2(1f, 0f));
        vertices.Add(position2);
        uvs.Add(new Vector2(0f, 1f));
        vertices.Add(position3);
        uvs.Add(new Vector2(1f, 1f));
        triangles.Add(count);
        triangles.Add(count + 1);
        triangles.Add(count + 2);
        triangles.Add(count + 3);
        triangles.Add(count + 2);
        triangles.Add(count + 1);
    }
    public void Clear()
    {
        meshFilter.mesh.Clear();
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
    }
   
}
