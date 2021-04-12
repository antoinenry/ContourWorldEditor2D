﻿using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class ContourMeshReader : ContourReader
{
    public Material MeshMaterial { get; protected set; }
    public List<Vector3> Vertices { get; protected set; }
    public List<int> Triangles { get; protected set; }
    public List<Vector3> Normals { get; protected set; }
    public List<Vector2> UVs { get; protected set; }
    public List<Color> Colors { get; protected set; }

    public override bool Clear()
    {
        if (MeshMaterial == null && Vertices == null && Triangles == null && Normals == null && UVs == null && Colors == null)
            return false;
        else
        {
            MeshMaterial = null;
            Vertices = null;
            Triangles = null;
            Normals = null;
            UVs = null;
            Colors = null;
            return true;
        }
    }

    public void ReadBlueprintNormal()
    {
        // Set normals: fetch value in blueprint
        if (blueprint != null && blueprint is ContourMeshBlueprint)
        {
            int vertexCount = Vertices != null ? Vertices.Count : 0;
            int normalCount = Normals != null ? Normals.Count : 0;
            if (vertexCount == normalCount && vertexCount > 0)
            {
                Vector3 normal = (blueprint as ContourMeshBlueprint).Normal;
                for (int i = 0; i < normalCount; i++)
                    Normals[i] = normal;
            }
            else throw new Exception("Blueprint and reader mismatch");
        }
        // Notify if there's a problem with the blueprint
        else throw new Exception("Can't read blueprint");
    }

    public void ReadBlueprintColor()
    {
        // Set colors: fetch color in blueprint
        if (blueprint != null && blueprint is ContourMeshBlueprint)
        {
            int vertexCount = Vertices != null ? Vertices.Count : 0;
            int colorCount = Colors != null ? Colors.Count : 0;
            if (vertexCount == colorCount && vertexCount > 0)
            {
                Color color = (blueprint as ContourMeshBlueprint).Color;
                for (int i = 0; i < colorCount; i++)
                    Colors[i] = color;
            }
            else throw new Exception("Blueprint and reader mismatch");
        }
        // Notify if there's a problem with the blueprint
        else throw new Exception("Can't read blueprint");
    }
}
