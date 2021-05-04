using UnityEngine;
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
    private enum UpdateType { None = 0, All = ~0, Positions = 2, Normals = 4, Colors = 8 }

    //public override void Build()
    //{
    //    int bpCount = blueprints != null ? blueprints.Count : 0;
    //    if (readers == null || readers.Count != blueprints.Count || blueprints.Contains(null)) ResetReaders();
    //    // Optimized update
    //    UpdateType requiredUpdates = UpdateType.None;
    //    for (int i = 0; i < bpCount; i++)
    //    {
    //        ContourBlueprint bp = blueprints[i];
    //        if (bp == null)
    //        {
    //            Debug.LogWarning("Null blueprint");
    //            continue;
    //        }
    //        ContourReader rd = readers[i];
    //        if (rd == null)
    //        {
    //            Debug.LogWarning("Null blueprint");
    //            continue;
    //        }
    //        ContourBlueprint.BlueprintChanges bpChanges = bp.changes;
    //        if (bpChanges != ContourBlueprint.BlueprintChanges.None)
    //        {
    //            if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.LengthChanged))
    //            {
    //                rd.TryReadBlueprint(bp);
    //                requiredUpdates = UpdateType.All;
    //            }
    //            if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.PositionMoved))
    //            {
    //                rd.ReadBlueprintPositions(bp);
    //                requiredUpdates |= UpdateType.Positions;
    //            }
    //            if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.ParameterChanged))
    //            {
    //                string[] changedParameters = bp.changedParameters.Split(' ');
    //                foreach (string p in changedParameters)
    //                {
    //                    switch (p)
    //                    {
    //                        case "normal":
    //                            (rd as ContourMeshReader).ReadBlueprintNormal(bp as ContourMeshBlueprint);
    //                            requiredUpdates |= UpdateType.Normals;
    //                            break;
    //                        case "color":
    //                            (rd as ContourMeshReader).ReadBlueprintColor(bp as ContourMeshBlueprint);
    //                            requiredUpdates |= UpdateType.Colors;
    //                            break;
    //                    }
    //                }
    //            }
    //            bp.changes = ContourBlueprint.BlueprintChanges.None;
    //            bp.changedParameters = "";
    //        }
    //    }
    //    // Apply required updates
    //    if (requiredUpdates == UpdateType.All)
    //        RebuildAll();
    //    else
    //    {
    //        if (requiredUpdates.HasFlag(UpdateType.Positions)) UpdatePositions();
    //        if (requiredUpdates.HasFlag(UpdateType.Normals)) UpdateNormals();
    //        if (requiredUpdates.HasFlag(UpdateType.Colors)) UpdateColors();
    //    }
    //}

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
            List<Material> submeshMaterials = submeshes.ConvertAll(sub => sub.submeshMaterial);
            render.materials = submeshMaterials.ToArray();
        }
    }

    protected override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourMeshReader;
    }

    public override void RebuildAll()
    {
        // Reread all blueprints
        ResetReaders();
        // Set submeshes
        submeshes = new List<ContourSubmeshBuilder>();
        if (readers != null)
        {
            foreach (ContourMeshReader reader in readers)
            {
                if (reader == null) continue;
                int subBuilderIndex = submeshes.FindIndex(sub => sub.submeshMaterial == reader.MeshMaterial);
                if (subBuilderIndex == -1)
                    submeshes.Add(new ContourSubmeshBuilder(reader));
                else
                    submeshes[subBuilderIndex].readers.Add(reader);
            }
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
        UpdateMeshComponents();
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
        UpdateMeshComponents();
    }

    private void UpdateNormals()
    {
        // Update mesh normals
        List<Vector3> normals = new List<Vector3>();
        foreach (ContourSubmeshBuilder sub in submeshes)
            normals.AddRange(sub.GetNormals());
        mesh.SetNormals(normals);
    }

    private void UpdateColors()
    {
        // Update mesh normals
        List<Color> colors = new List<Color>();
        foreach (ContourSubmeshBuilder sub in submeshes)
            colors.AddRange(sub.GetColors());
        mesh.SetColors(colors);
    }

    protected override void OnChangeBlueprintParameters()
    {
        RebuildAll();
    }
}
