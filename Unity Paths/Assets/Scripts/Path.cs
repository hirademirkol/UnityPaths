using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[ExecuteInEditMode]
public class Path
{
    [SerializeField, HideInInspector]
    List<Vector3> _points;
    bool _looping;

    Transform _transform;

    public delegate void ChangeDelegate();

    public ChangeDelegate HandleChange;

    public Path(Transform transform, bool looping)
    {
        _transform = transform;

        _points = new List<Vector3>();
        _points.Add(Vector3.left);
        _points.Add((Vector3.left + Vector3.up) / 2);
        _points.Add((Vector3.right + Vector3.up) / 2);
        _points.Add(Vector3.right);
        if(looping)
        {
            _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
            _points.Add((_points[0] * 2 - _points[1]));
        }

        _looping = looping;
    }

    public bool Looping
    {
        get
        {
            return _looping;
        }
        set
        {
            _looping = value;
            if(_looping)
            {
                if(_points.Count % 3 != 0)
                {
                    _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
                    _points.Add((_points[0] * 2 - _points[1]));
                }
            }
            else
            {
                if(_points.Count % 3 == 0)
                    _points.RemoveRange(_points.Count-2, 2);
            }
            ChangeMade();
        }
    }

    public bool IsNull()
    {
        return _transform == null || _points == null;
    }
    public Vector3 this[int i]
    {
        get
        {
            return _transform.TransformPoint(_points[i]);
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

    int LoopIndex(int i)
    {
        return (i + _points.Count) % (_points.Count);
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] {this[LoopIndex(i*3)], this[LoopIndex(i*3+1)], this[LoopIndex(i*3+2)], this[LoopIndex(i*3+3)]};
    }

    public void MovePoint(int pointIndex, Vector3 newPos)
    {
        newPos = _transform.InverseTransformPoint(newPos);

        if(Looping)
            pointIndex = LoopIndex(pointIndex);

        _points[pointIndex] = newPos;
        ChangeMade();
    }

    public void MovePointWith(int pointIndex, Vector3 moveVector)
    {
        moveVector = _transform.InverseTransformVector(moveVector);

        if(Looping)
            pointIndex = LoopIndex(pointIndex);

        _points[pointIndex] += moveVector;
        ChangeMade();
    }

    public void AddSegment(Vector3 endPos)
    {
        endPos = _transform.InverseTransformPoint(endPos);

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

        float stepSize = Looping ? 1/(float)n : 1/(float)(n-1);

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
                if(i == n || (i == n-1 && !Looping)) break;
            }
            if(i == n || (i == n-1 && !Looping)) break;
        }

        if(!Looping)
        {
            points[n-1] = _points[NumPoints-1];
            tangents[n-1] = GetDerivative(NumSegments-1, 1);
            normals[n-1] = GetProjectedNormal(normals[n-2], tangents[n-1]);
        }

        return points;
    }

    Vector3 Evaluate(int segment, float t)
    {
        return Vector3.Lerp(QuadLerp(_points[LoopIndex(segment * 3)], _points[LoopIndex(segment * 3 + 1)], _points[LoopIndex(segment * 3 + 2)], t),
                            QuadLerp(_points[LoopIndex(segment * 3 + 1)], _points[LoopIndex(segment * 3 + 2)], _points[LoopIndex(segment * 3 + 3)], t), t);
    }

    Vector3 QuadLerp(Vector3 p1, Vector3 p2, Vector3 p3,float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p1, p2, t), Vector3.Lerp(p2, p3, t), t);
    }

    //TODO: Handle Derivative (maybe also actual) calculations with better mathematics
    Vector3 GetDerivative(int segment, float t)
    {
        return QuadLerp(_points[LoopIndex(segment * 3 + 1)] - _points[LoopIndex(segment * 3)],
                        _points[LoopIndex(segment * 3 + 2)] - _points[LoopIndex(segment * 3 + 1)],
                        _points[LoopIndex(segment * 3 + 3)] - _points[LoopIndex(segment * 3 + 2)], t);
    }

    Vector3 GetSecondDerivative(int segment, float t)
    {
        return Vector3.Lerp(_points[LoopIndex(segment * 3 + 2)] - 2*_points[LoopIndex(segment * 3 + 1)] + _points[LoopIndex(segment * 3)],
                            _points[LoopIndex(segment * 3 + 3)] - 2*_points[LoopIndex(segment * 3 + 2)] - _points[LoopIndex(segment * 3 + 1)], t);
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
        float netLength = Vector3.Distance(this[LoopIndex(segment * 3)], this[LoopIndex(segment * 3 + 1)]) + 
                          Vector3.Distance(this[LoopIndex(segment * 3 + 1)], this[LoopIndex(segment * 3 + 2)]) + 
                          Vector3.Distance(this[LoopIndex(segment * 3 + 2)], this[LoopIndex(segment * 3 + 3)]);
        return Vector3.Distance(this[LoopIndex(segment * 3)], this[LoopIndex(segment * 3 + 3)]) + netLength * 0.5f;
    }

    //Approximates total length using shortened version of ApproximateLength
    public float ApproximateTotalLength()
    {
        float length = 0f;
        for(int i = 0; i < NumPoints-1; i++)
        {
            length += Vector3.Distance(this[i], this[LoopIndex(i+1)]) * 0.5f;
            if(i % 3 == 0)
                length += Vector3.Distance(this[i], this[LoopIndex(i+3)]);
        }
        if(Looping)
            length += Vector3.Distance(this[NumPoints-1], this[0]) * 0.5f;
        return length;
    }
}
