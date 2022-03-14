using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[System.Serializable]
[ExecuteInEditMode]
public class PathInstancer : PathSampler
{
    [SerializeField, HideInInspector]
    public Mesh mesh;
    public Mesh InstancedMesh;

    public float Scale = 1f;

    public Transform InstanceTransform;

    float _oldScale;
    Matrix4x4 _oldTransformMatrix;

    protected override bool _changeMade 
    {
        get
        {
            return base._changeMade || _oldScale != Scale || _oldTransformMatrix != InstanceTransform.worldToLocalMatrix;
        }
    }

    void Reset()
    {
        if(InstancedMesh && InstanceTransform)
            Initialize();
    }
    void OnValidate()
    {
        if(InstancedMesh && InstanceTransform)
            UpdateMesh();
    }

    protected override void Initialize()
    {
        base.Initialize();
        _oldScale = Scale;
        InstanceTransform.position = _path[0];
        _oldTransformMatrix = InstanceTransform.worldToLocalMatrix;
        CalculateDivisions();

        base.HandleChange += HandleChange;
        base._path.HandleChange += HandleChange;

        if(mesh == null)
            mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
        InstanceOnPath();
    }

    void UpdateMesh()
    {
        _oldScale = Scale;
        _oldTransformMatrix = InstanceTransform.worldToLocalMatrix;
        CalculateDivisions();
        InstanceOnPath();
    }

    public new void HandleChange()
    {
        UpdateMesh();
    }

    void InstanceOnPath()
    {
        SamplePath();
        CombineInstance[] instances = new CombineInstance[PathDivisions];
        
        for(int i = 0; i < PathDivisions; i++)
        {
            instances[i].mesh = InstancedMesh;
            instances[i].transform = Matrix4x4.TRS(_samplePoints[i], InstanceTransform.localRotation * Quaternion.FromToRotation(Vector3.right, _normals[i]), Scale * InstanceTransform.localScale);
            
        }

        mesh.Clear();
        mesh.CombineMeshes(instances);
    }

    void CalculateDivisions()
    {
        float unitLength = InstancedMesh.bounds.size.y * Scale;
        PathDivisions = Mathf.FloorToInt(_path.ApproximateTotalLength()/unitLength);
    }
}
