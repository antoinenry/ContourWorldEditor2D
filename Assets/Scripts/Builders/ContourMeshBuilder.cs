using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ContourMeshBuilder : ContourBuilder
{
    private List<ContourSubmeshBuilder> submeshes;
    private Mesh mesh;
    private MeshFilter filter;
    private MeshRenderer render;

    protected void UpdateComponents()
    {
        if (filter == null)
            filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            filter.sharedMesh = mesh;
        }
        if (render == null)
            render = GetComponent<MeshRenderer>();
        if (render != null)
        {
            List<Material> submeshMaterials = submeshes.ConvertAll(sub => sub.submeshMaterial);
            render.materials = submeshMaterials.ToArray();
        }
    }

    public static ContourMeshBuilder AddBuilderComponent(ContourMeshReader meshReader, GameObject go)
    {
        go.name = "Mesh builder";
        ContourMeshBuilder newBuilder = go.AddComponent<ContourMeshBuilder>();
        return newBuilder;
    }

    public override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourMeshReader;
    }

    public override void Build()
    {
        // Set submeshes
        submeshes = new List<ContourSubmeshBuilder>();
        foreach (ContourMeshReader reader in readers)
        {
            if (reader == null) continue;
            int subBuilderIndex = submeshes.FindIndex(sub => sub.submeshMaterial == reader.MeshMaterial);
            if (subBuilderIndex == -1)
                submeshes.Add(new ContourSubmeshBuilder(reader));
            else
                submeshes[subBuilderIndex].readers.Add(reader);
        }
        // Build mesh
        mesh = new Mesh();
        // Set submeshes
        int submeshCount = submeshes.Count;
        mesh.subMeshCount = submeshCount;
        // Set vertices, normals, uvs and colors: all submeshes in a row
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();
        foreach (ContourSubmeshBuilder sub in submeshes)
        {
            vertices.AddRange(sub.GetVertices());
            normals.AddRange(sub.GetNormals());
            uvs.AddRange(sub.GetUVs());
            colors.AddRange(sub.GetColors());
        }
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        // Set triangles: one submesh at a time, with an offset each time we go to the next submesh
        int startIndex = 0;
        for (int i = 0; i < submeshCount; i++)
        {
            List<int> submeshTriangles = submeshes[i].GetTriangles(startIndex, out int endIndex);
            mesh.SetTriangles(submeshTriangles, i);
            startIndex = endIndex;
        }
        // Set bounds
        mesh.RecalculateBounds();
        // Update mesh filter and renderer
        UpdateComponents();
    }
}
