using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathObject : MonoBehaviour
{
    [HideInInspector]
    public Path Path;

    void Reset()
    {
        CreatePath();
    }
    void Awake()
    {
        CreatePath();
    }
    
    public void CreatePath()
    {
        Path = new Path(transform.position);
    }
}
