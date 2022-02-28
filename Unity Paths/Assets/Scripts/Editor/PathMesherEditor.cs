using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathMesher))]
public class PathMesherEditor : Editor
{
    PathMesher pathMesher;

    void OnEnable()
    {
        pathMesher = (PathMesher)target;
    }

    void OnSceneGUI()
    {
        DrawNormals();
    }

    void DrawNormals()
    {
        Handles.color = Color.yellow;
        if(pathMesher.SamplePoints != null)
        {
            for(int i = 0; i < pathMesher.SamplePoints.Length; i++)
            {
                Handles.DrawLine(pathMesher.SamplePoints[i],pathMesher.SamplePoints[i] + pathMesher.Normals[i] * 0.2f);
            }
        }
    }
}
