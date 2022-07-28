using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ContourMeshBuilder : ContourBuilder
{
    [Serializable]
    private class SubmeshBuilder
    {
        public readonly Material submeshMaterial;
        public List<ContourMeshReader> readers;

        public SubmeshBuilder(ContourMeshReader reader)
        {
            submeshMaterial = reader != null ? reader.MeshMaterial : null;
            readers = new List<ContourMeshReader>() { reader };
        }

        public List<Vector3> GetVertices()
        {
            // All readers in a row
            List<Vector3> vertices = new List<Vector3>();
            foreach (ContourMeshReader r in readers)
            {
                if (r != null && r.Vertices != null)
                    vertices.AddRange(r.Vertices);
            }
            return vertices;
        }

        public List<int> GetTriangles(int startIndex, out int endIndex)
        {
            // All readers in a row, but we add an offset to the indices each time we change reader
            int offset = startIndex;
            List<int> triangles = new List<int>();
            foreach (ContourMeshReader r in readers)
            {
                if (r != null && r.Triangles != null)
                {
                    triangles.AddRange(r.Triangles.ConvertAll(index => index + offset));
                    offset += r.Vertices.Count;
                }
            }
            endIndex = offset;
            return triangles;
        }

        public List<Vector3> GetNormals()
        {
            // All readers in a row
            List<Vector3> normals = new List<Vector3>();
            foreach (ContourMeshReader r in readers)
            {
                if (r != null && r.Normals != null)
                    normals.AddRange(r.Normals);
            }
            return normals;
        }

        public List<Vector2> GetUVs()
        {
            // All readers in a row
            List<Vector2> uvs = new List<Vector2>();
            foreach (ContourMeshReader r in readers)
            {
                if (r != null && r.UVs != null)
                    uvs.AddRange(r.UVs);
            }
            return uvs;
        }

        public List<Color> GetColors()
        {
            // All readers in a row
            List<Color> colors = new List<Color>();
            foreach (ContourMeshReader r in readers)
            {
                if (r != null && r.Colors != null)
                    colors.AddRange(r.Colors);
            }
            return colors;
        }
    }

    private List<SubmeshBuilder> subBuilders;
    private Mesh mesh;
    private MeshFilter filter;
    private MeshRenderer render;

    [Flags]
    private enum UpdateType { None = 0, All = ~0, Positions = 2, Normals = 4, Colors = 8 }

    public override void RebuildAll()
    {
        ResetReaders();
        // Set subbuilders
        subBuilders = new List<SubmeshBuilder>();
        if (readers != null)
        {
            foreach (ContourMeshReader reader in readers)
            {
                if (reader == null) continue;                
                int subBuilderIndex = subBuilders.FindIndex(sub => sub.submeshMaterial == reader.MeshMaterial);
                if (subBuilderIndex == -1)
                    subBuilders.Add(new SubmeshBuilder(reader));
                else
                    subBuilders[subBuilderIndex].readers.Add(reader);
            }
        }
        // Build mesh
        mesh = new Mesh();
        // Set submeshes
        int submeshCount = subBuilders.Count;
        mesh.subMeshCount = submeshCount;
        // Set vertices, normals, uvs and colors: all submeshes in a row
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();
        foreach (SubmeshBuilder sub in subBuilders)
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
            List<int> submeshTriangles = subBuilders[i].GetTriangles(startIndex, out int endIndex);
            mesh.SetTriangles(submeshTriangles, i);
            startIndex = endIndex;
        }
        // Set bounds
        mesh.RecalculateBounds();
        // Update mesh filter and renderer
        UpdateMeshComponents();
    }

    protected override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourMeshReader;
    }

    protected override void UpdatePositions()
    {
        if (mesh == null)
        {
            RebuildAll();
            return;
        }
        // Update mesh vertices
        List<Vector3> vertices = new List<Vector3>();
        foreach (SubmeshBuilder sub in subBuilders)
        {
            if (sub.readers != null) vertices.AddRange(sub.GetVertices());
            else
            {
                RebuildAll();
                return;
            }
        }
        mesh.SetVertices(vertices);
        // Set bounds
        mesh.RecalculateBounds();
        // Update mesh filter and renderer
        UpdateMeshComponents();
    }

    protected override void UpdateNormals()
    {
        if (mesh == null)
        {
            RebuildAll();
            return;
        }
        // Update mesh normals
        List<Vector3> normals = new List<Vector3>();
        foreach (SubmeshBuilder sub in subBuilders)
        {
            if (sub.readers != null) normals.AddRange(sub.GetNormals());
            else
            {
                RebuildAll();
                return;
            }
        }
        mesh.SetNormals(normals);
    }

    private void UpdateMeshComponents()
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
            List<Material> submeshMaterials = subBuilders.ConvertAll(sub => sub.submeshMaterial);
            render.materials = submeshMaterials.ToArray();
        }
    }
}
