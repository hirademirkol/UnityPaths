using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathObject : MonoBehaviour
{
    [HideInInspector]
    public Path Path;

    void Reset()
    {
        CreatePath();
    }
    
    public void CreatePath()
    {
        Path = new Path(transform.position);
    }
}
