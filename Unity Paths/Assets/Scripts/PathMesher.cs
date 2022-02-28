using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathObject))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[System.Serializable]
[ExecuteInEditMode]
public class PathMesher : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public Mesh Mesh;

    public int PathDivisions = 10;
    public int VerticalDivisions = 6;
    public float Radius = 0.1f;

    public Vector3[] SamplePoints
    {
        get
        {
            return _samplePoints;
        }
    }
    public Vector3[] Normals
    {
        get
        {
            return _normals;
        }
    }
    public Vector3[] Tangents
    {
        get
        {
            return _tangents;
        }
    }

    Path _path;
    int _oldPathDiv;
    int _oldVerDiv;
    float _oldRad;
    Vector3[] _samplePoints;
    Vector3[] _normals, _tangents;

    bool _changed
    {
        get
        {
            return PathDivisions != _oldPathDiv || VerticalDivisions != _oldVerDiv || Radius != _oldRad || _path.ChangeMade;
        }
    }

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        if(_changed)
        {
            _oldPathDiv = PathDivisions;
            _oldVerDiv = VerticalDivisions;
            _oldRad = Radius;
            _path.ChangeMade = false;
            MeshPath();
        }
    }
    
    void Reset()
    {
        Initialize();
    }

    void Initialize()
    {
        _oldPathDiv = PathDivisions;
        _oldVerDiv = VerticalDivisions;
        _oldRad = Radius;
        
        if(_path == null)
            _path = GetComponent<PathObject>().Path;
        if(Mesh == null)
            Mesh = new Mesh();
        
        GetComponent<MeshFilter>().mesh = Mesh;
        MeshPath();
    }

    void MeshPath()
    {
        _samplePoints = _path.Sample(PathDivisions, out _tangents, out _normals);

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


        Mesh.Clear();
        Mesh.vertices = verts;
        Mesh.triangles = tris;
        Mesh.RecalculateNormals();
    }
}
