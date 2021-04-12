﻿using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ContourMeshBuilder : ContourBuilder
{
    private List<ContourSubmeshBuilder> submeshes;
    private Mesh mesh;
    private MeshFilter filter;
    private MeshRenderer render;

    [Flags]
    private enum UpdateType { None = 0, All = ~0, Positions = 2, Normals = 4 }

    public override void Update()
    {
        // Optimized update
        if (readers != null)
        {
            // Evaluate what needs to be updated
            UpdateType requiredUpdates = UpdateType.None;
            foreach (ContourMeshReader rd in readers)
            {
                if (rd == null || rd.Blueprint == null) continue;
                ContourBlueprint.BlueprintChanges bpChanges = rd.Blueprint.changes;
                if (bpChanges != ContourBlueprint.BlueprintChanges.None)
                {
                    if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.LengthChanged))
                    {
                        rd.ReadBlueprint();
                        requiredUpdates = UpdateType.All;
                    }
                    if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.PositionMoved))
                    {
                        rd.ReadBlueprintPositions();
                        requiredUpdates |= UpdateType.Positions;
                    }
                    if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.ParameterChanged))
                    {
                        string[] changedParameters = rd.Blueprint.changedParameters.Split(' ');
                        foreach(string p in changedParameters)
                        {
                            switch(p)
                            {
                                case "normal":
                                    rd.ReadBlueprintNormal();
                                    requiredUpdates |= UpdateType.Normals;
                                    break;
                            }
                        }
                    }
                    rd.Blueprint.changes = ContourBlueprint.BlueprintChanges.None;
                    rd.Blueprint.changedParameters = "";
                }
            }
            // Apply required updates
            if (requiredUpdates == UpdateType.All)
                Build();
            else
            {
                if (requiredUpdates.HasFlag(UpdateType.Positions)) UpdatePositions();
                if (requiredUpdates.HasFlag(UpdateType.Normals)) UpdateNormals();
            }
        }
    }

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

    public override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourMeshReader;
    }

    public override void Build()
    {
        Debug.Log("Build mesh");
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

    protected override void UpdatePositions()
    {
        // Update mesh vertices
        List<Vector3> vertices = new List<Vector3>();
        foreach (ContourSubmeshBuilder sub in submeshes)
            vertices.AddRange(sub.GetVertices());
        mesh.SetVertices(vertices);
        // Set bounds
        mesh.RecalculateBounds();
        // Update mesh filter and renderer
        UpdateComponents();
    }

    private void UpdateNormals()
    {
        // Update mesh normals
        List<Vector3> normals = new List<Vector3>();
        foreach (ContourSubmeshBuilder sub in submeshes)
            normals.AddRange(sub.GetNormals());
        mesh.SetNormals(normals);
    }

    protected override void OnChangeBlueprintParameters()
    {
        Build();
    }
}
