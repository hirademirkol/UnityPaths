using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[ExecuteInEditMode]
public class Path
{
    [SerializeField, HideInInspector]
    List<Vector3> _points;

    public delegate void ChangeDelegate();

    public ChangeDelegate HandleChange;

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

    void ChangeMade()
    {
        HandleChange?.Invoke();
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] {_points[i*3], _points[i*3 + 1], _points[i*3 + 2], _points[i*3 + 3]};
    }

    public void MovePoint(int pointIndex, Vector3 newPos)
    {
        _points[pointIndex] = newPos;
        ChangeMade();
    }

    public void MovePointWith(int pointIndex, Vector3 moveVector)
    {
        _points[pointIndex] += moveVector;
        ChangeMade();
    }

    public void AddSegment(Vector3 endPos)
    {
        _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
        _points.Add((_points[_points.Count - 1] + endPos) * .5f);
        _points.Add(endPos);
        ChangeMade();
    }

    public Vector3[] Sample(int n, out Vector3[] tangents, out Vector3[] normals)
    {
        normals = new Vector3[n];
        tangents = new Vector3[n];
        Vector3[] points = new Vector3[n];

        float stepSize = 1/(float)(n-1);

        points[0] = Evaluate(0, 0f);
        tangents[0] = GetDerivative(0, 0f).normalized;
        normals[0] = GetNormal(0, 0f);

        float progress = stepSize;
        float minProgress;
        float maxProgress = 0f;

        //Approximate progress from approximated segment lengths, evaluate accordingly        
        for(int i = 1, j = 0; j < NumSegments; j++)
        {
            minProgress = maxProgress;
            maxProgress += ApproximateLength(j)/ApproximateTotalLength();
            while(progress < maxProgress)
            {
                float p = Mathf.InverseLerp(minProgress,maxProgress,progress);
                points[i] = Evaluate(j, p);
                tangents[i] = GetDerivative(j, p).normalized;//(points[i] + points[i-2]).normalized;
                normals[i] = GetProjectedNormal(normals[i-1], tangents[i]);
                progress += stepSize;
                i++;
                if(i == n-1) break;
            }
            if(i == n-1) break;
            //progress %= 1f;
        }

        points[n-1] = _points[NumPoints-1];
        tangents[n-1] = GetDerivative(NumSegments-1, 1);
        normals[n-1] = GetProjectedNormal(normals[n-2], tangents[n-1]);
        return points;
    }

    Vector3 Evaluate(int segment, float t)
    {
        return Vector3.Lerp(QuadLerp(_points[segment * 3], _points[segment * 3 + 1], _points[segment * 3 + 2], t),
                            QuadLerp(_points[segment * 3 + 1], _points[segment * 3 + 2], _points[segment * 3 + 3], t), t);
    }

    Vector3 QuadLerp(Vector3 p1, Vector3 p2, Vector3 p3,float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p1, p2, t), Vector3.Lerp(p2, p3, t), t);
    }

    //TODO: Handle Derivative (maybe also actual) calculations with better mathematics
    Vector3 GetDerivative(int segment, float t)
    {
        return QuadLerp(_points[segment * 3 + 1] - _points[segment * 3],
                        _points[segment * 3 + 2] - _points[segment * 3 + 1],
                        _points[segment * 3 + 3] - _points[segment * 3 + 2], t);
    }

    Vector3 GetSecondDerivative(int segment, float t)
    {
        return Vector3.Lerp(_points[segment * 3 + 2] - 2*_points[segment * 3 + 1] + _points[segment * 3],
                            _points[segment * 3 + 3] - 2*_points[segment * 3 + 2] - _points[segment * 3 + 1], t);
    }

    //Get 3D Normal using Bezier mathematics
    Vector3 GetNormal(int segment, float t)
    {
        Vector3 a = GetDerivative(segment, t).normalized;
        Vector3 b = (a + GetSecondDerivative(segment, t)).normalized;
        Vector3 r = Vector3.Cross(b, a).normalized;
        return Vector3.Cross(r, a).normalized;
    }

    //Get 3D Projected normal using previous normal and projecting it on the plane with the tangent as the normal
    //Gives consistent normals and works-around inevitable twisting of the normals
    Vector3 GetProjectedNormal(Vector3 prevNormal, Vector3 tangent)
    {
        return Vector3.ProjectOnPlane(prevNormal, tangent).normalized;
    }

    //Approximates the segment length using length of the boundary net
    public float ApproximateLength(int segment)
    {
        float netLength = Vector3.Distance(_points[segment * 3], _points[segment * 3 + 1]) + 
                          Vector3.Distance(_points[segment * 3 + 1], _points[segment * 3 + 2]) + 
                          Vector3.Distance(_points[segment * 3 + 2], _points[segment * 3 + 3]);
        return Vector3.Distance(_points[segment * 3], _points[segment * 3 + 3]) + netLength * 0.5f;
    }

    //Approximates total length using shortened version of ApproximateLength
    public float ApproximateTotalLength()
    {
        float length = 0f;
        for(int i = 0; i < NumPoints-1; i++)
        {
            length += Vector3.Distance(_points[i], _points[i+1]) * 0.5f;
            if(i % 3 == 0)
                length += Vector3.Distance(_points[i], _points[i+3]);
        }
        return length;
    }
}
