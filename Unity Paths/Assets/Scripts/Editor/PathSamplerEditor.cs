using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathSampler), true)]
public class PathMesherEditor : Editor
{
    PathSampler pathSampler;

    void OnEnable()
    {
        pathSampler = (PathSampler)target;
    }

    void OnSceneGUI()
    {
        DrawNormals();
    }

    void DrawNormals()
    {
        if(pathSampler.SamplePoints != null)
        {
            Handles.color = Color.yellow;
            for(int i = 0; i < pathSampler.SamplePoints.Length; i++)
            {
                Handles.DrawLine(pathSampler.SamplePoints[i],pathSampler.SamplePoints[i] + pathSampler.Normals[i] * 0.2f);
            }
        }
    }
}
