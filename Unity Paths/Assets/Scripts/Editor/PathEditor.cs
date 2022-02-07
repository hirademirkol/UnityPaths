using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathObject))]
public class PathEditor : Editor
{
    PathObject pathObject;
    Path Path
    {
        get
        {
            return pathObject.Path;
        }
    }

    Plane _hitPlane;

    void OnEnable()
    {
        pathObject = (PathObject)target;
        if(Path == null)
            pathObject.CreatePath();
    }

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        
        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            float enter = 0f;
            _hitPlane.Set3Points(Path[Path.NumPoints-4], Path[Path.NumPoints-2], Path[Path.NumPoints-1]);
            Undo.RecordObject(pathObject, "Add Segment");
            _hitPlane.Raycast(mouseRay, out enter);
            Path.AddSegment(mouseRay.GetPoint(enter));
        }
    }

    void Draw()
    {
        for(int i = 0; i < Path.NumSegments; i++)
        {
            Vector3[] p = Path.GetPointsInSegment(i);
            Handles.color = Color.black;
            Handles.DrawLine(p[0], p[1]);
            Handles.DrawLine(p[2], p[3]);
            Handles.DrawBezier(p[0], p[3], p[1], p[2], Color.green, null, 2);
        }

        for(int i = 0; i < Path.NumPoints; i++)
        {
            Handles.color = (i % 3 == 0) ? Color.red : Color.blue;
            Vector3 newPos = Handles.FreeMoveHandle(Path[i], Quaternion.identity, .1f, Vector3.zero, Handles.SphereHandleCap);
            if(Path[i] != newPos)
            {
                Undo.RecordObject(pathObject, "Move Handle");
                
                if(i % 3 == 0)
                {
                    Vector3 moveVector = newPos - Path[i];
                    if(i != 0)
                        Path.MovePointWith(i-1, moveVector);
                    if(i != Path.NumPoints - 1)
                        Path.MovePointWith(i+1, moveVector);
                }
                Path.MovePoint(i, newPos);
            }
        }
    }
}
