using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathObject))]
[ExecuteInEditMode]
public class PathSampler : MonoBehaviour
{
    public int PathDivisions = 10;
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

    protected Path _path;
    protected Vector3[] _samplePoints;
    protected Vector3[] _normals, _tangents;

    public delegate void ChangeDelegate();

    public ChangeDelegate HandleChange;

    int _oldPathDiv;
    
    protected virtual bool _changeMade
    {
        get
        {
            return _oldPathDiv != PathDivisions;
        }
    } 

    void OnEnable()
    {
        Initialize();
    }

    void Awake()
    {
        Initialize();
    }

    void Update()
    {
        if(_changeMade)
            HandleChange?.Invoke();
    }

    protected void SamplerChanged()
    {
        _oldPathDiv = PathDivisions;
    }

    protected virtual void Initialize()
    {
        _oldPathDiv = PathDivisions;

        GetComponent<PathObject>().CreatePath();
        _path = GetComponent<PathObject>().Path;
    }

    protected void SamplePath()
    {
        _samplePoints = _path.Sample(PathDivisions, out _tangents, out _normals);
    }
}
