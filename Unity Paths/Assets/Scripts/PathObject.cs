using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[System.Serializable]
public class PathObject : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public Path Path;
    public bool Looping;

    void Reset()
    {
        Path = new Path(transform, Looping);
    }

    void OnEnable()
    {
        CreatePath();
    }
    void OnValidate()
    {
        Path.Looping = Looping;
    }

    void Awake()
    {
        CreatePath();
    }
    
    public void CreatePath()
    {
        if(Path.IsNull())
            Path = new Path(transform, Looping);
    }
}
