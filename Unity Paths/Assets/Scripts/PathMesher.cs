using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[System.Serializable]
[ExecuteInEditMode]
public class PathMesher : PathSampler
{
    [SerializeField, HideInInspector]
    public Mesh mesh;

    public int VerticalDivisions = 6;
    public float Radius = 0.1f;

    int _oldVerDiv;
    float _oldRad;

    protected override bool _changeMade 
    {
        get
        {
            return base._changeMade || _oldVerDiv != VerticalDivisions || _oldRad != Radius;
        }
    }

    void Reset()
    {
        Initialize();
    }
    
    void OnValidate()
    {
        UpdateMesh();
    }

    protected override void Initialize()
    {
        base.Initialize();
        _oldVerDiv = VerticalDivisions;
        _oldRad = Radius;

        base.HandleChange += HandleChange;
        base._path.HandleChange += HandleChange;
        
        if(mesh == null)
            mesh = new Mesh();
        
        GetComponent<MeshFilter>().mesh = mesh;
        MeshPath();
    }

    void UpdateMesh()
    {
        _oldVerDiv = VerticalDivisions;
        _oldRad = Radius;
        SamplerChanged();
        MeshPath();
    }

    public new void HandleChange()
    {
        UpdateMesh();
    }

    void MeshPath()
    {
        SamplePath();
        Vector3[] verts = new Vector3[VerticalDivisions * PathDivisions];
        int[] tris = new int[6 * VerticalDivisions * (PathDivisions-1)];

        float rotation = 360/(float)VerticalDivisions; 

        int  tri = 0, vert = 0;
        Vector3 side;

        for(int i = 0; i < PathDivisions - 1; i++)
        {
            side = _normals[i];
            for(int j = 0; j < VerticalDivisions-1; j++)
            {
                verts[vert] = _samplePoints[i] + Radius * side;
                tris[tri] = vert;
                tris[tri+1] = vert + VerticalDivisions + 1;
                tris[tri+2] = vert + VerticalDivisions;
                tris[tri+3] = vert;
                tris[tri+4] = vert + 1;
                tris[tri+5] = vert + VerticalDivisions + 1;
                vert++;
                tri+=6;
                
                //Rotate the normal to get a circle with VerticalDivisions points around the sample point
                side = Quaternion.AngleAxis(rotation, _tangents[i]) * side;
                side.Normalize();
            }

            //Connect last point with first point and next circle points
            verts[vert] = _samplePoints[i] + Radius * side;
            tris[tri] = vert;
            tris[tri+1] = vert + 1;
            tris[tri+2] = vert + VerticalDivisions;
            tris[tri+3] = vert;
            tris[tri+4] = vert + 1 - VerticalDivisions;
            tris[tri+5] = vert + 1;
            vert++;
            tri+=6;
        }

        //Only add the vertices at the last circle
        side = _normals[PathDivisions-1];
        for(int j = 0; j < VerticalDivisions; j++)
        {
            verts[vert] = _samplePoints[PathDivisions-1] + Radius * side;
            vert++;

            side = Quaternion.AngleAxis(rotation, _tangents[PathDivisions-1]) * side;
            side.Normalize();
        }


        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }
}
