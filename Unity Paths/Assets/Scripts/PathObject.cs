using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[System.Serializable]
public class PathObject : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public Path Path;

    void Reset()
    {
        Path = new Path(transform.position);
    }

    void OnEnable()
    {
        CreatePath();
    }

    void Awake()
    {
        CreatePath();
    }
    
    public void CreatePath()
    {
        if(Path == null)
            Path = new Path(transform.position);
    }
}
