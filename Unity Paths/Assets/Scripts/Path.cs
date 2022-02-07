using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    List<Vector3> _points;

    public Path(Vector3 start)
    {
        _points = new List<Vector3>();
        _points.Add(start + Vector3.left);
        _points.Add(start + (Vector3.left + Vector3.up) / 2);
        _points.Add(start + (Vector3.right + Vector3.down) / 2);
        _points.Add(start + Vector3.right);
    }

    public Vector3 this[int i]
    {
        get
        {
            return _points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return _points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return _points.Count / 3;
        }
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] {_points[i*3], _points[i*3 + 1], _points[i*3 + 2], _points[i*3 + 3]};
    }

    public void MovePoint(int pointIndex, Vector3 newPos)
    {
        _points[pointIndex] = newPos;
    }

    public void MovePointWith(int pointIndex, Vector3 moveVector)
    {
        _points[pointIndex] += moveVector;
    }

    public void AddSegment(Vector3 endPos)
    {
        _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
        _points.Add((_points[_points.Count - 1] + endPos) * .5f);
        _points.Add(endPos);
    }
}
