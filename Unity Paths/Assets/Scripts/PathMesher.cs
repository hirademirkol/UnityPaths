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
    public bool ClosedCaps = true;

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
        _initialized = false;
        Initialize();
    }
    
    void OnValidate()
    {
        if(_initialized)
            UpdateMesh();
    }

    protected override void Initialize()
    {
        base.Initialize();
        _oldVerDiv = VerticalDivisions;
        _oldRad = Radius;

        base.HandleChange += HandleChange;
        _path.HandleChange += HandleChange;
        
        if(mesh == null)
            mesh = new Mesh();
        
        GetComponent<MeshFilter>().mesh = mesh;
        _initialized = true;
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
        Vector3[] verts;
        int[] tris;

        if(_path.Looping)
        {
            verts = new Vector3[VerticalDivisions * PathDivisions];
            tris = new int[6 * VerticalDivisions * PathDivisions];
        }
        else
        {
            if(ClosedCaps)
            {
                verts = new Vector3[VerticalDivisions * PathDivisions + 2];
                tris =  new int[6 * VerticalDivisions * PathDivisions];
            }
            else
            {
                verts = new Vector3[VerticalDivisions * PathDivisions];
                tris = new int[6 * VerticalDivisions * (PathDivisions-1)];
            }
        }

        float rotation = 360/(float)VerticalDivisions; 

        int  tri = 0, vert = 0;
        Vector3 side;


        if(ClosedCaps && !_path.Looping)
        {
            verts[0] = _samplePoints[0];
            for(int j = 0; j < VerticalDivisions-1; j++)
            {   
                tris[tri] = 0;
                tris[tri+1] = j + 2;
                tris[tri+2] = j + 1;
                tri+=3;
            }
               
            tris[tri] = 0;
            tris[tri+1] = 1;
            tris[tri+2] = VerticalDivisions;
            tri+=3;
            vert++;
        }

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

        //Only add the vertices at the last circle, add triangles if path is looping
        side = _normals[PathDivisions-1];
        for(int j = 0; j < VerticalDivisions-1; j++)
        {
            verts[vert] = _samplePoints[PathDivisions-1] + Radius * side;

            if(_path.Looping)
            {
                tris[tri] = vert;
                tris[tri+1] = j + 1;
                tris[tri+2] = j;
                tris[tri+3] = vert;
                tris[tri+4] = vert + 1;
                tris[tri+5] = j + 1;
                tri+=6;
            }
            vert++;

            side = Quaternion.AngleAxis(rotation, _tangents[PathDivisions-1]) * side;
            side.Normalize();
        }

        verts[vert] = _samplePoints[PathDivisions - 1] + Radius * side;
        
        //Connect last point with first point and next circle points
        if(_path.Looping)
        {
            tris[tri] = vert;
            tris[tri+1] = 0;
            tris[tri+2] = VerticalDivisions - 1;
            tris[tri+3] = vert;
            tris[tri+4] = vert - VerticalDivisions + 1;
            tris[tri+5] = 0;
            tri+=6;
        }
        vert++;

        if(ClosedCaps && !_path.Looping)
        {
            verts[vert] = _samplePoints[PathDivisions-1];
            for(int j = 0; j < VerticalDivisions-1; j++)
            {   
                tris[tri] = vert;
                tris[tri+1] = vert - j - 2;
                tris[tri+2] = vert - j - 1;
                tri+=3;
            }
               
            tris[tri] = vert;
            tris[tri+1] = vert - 1;
            tris[tri+2] = vert - VerticalDivisions;
            tri+=3;
            vert++;
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
    }
}
