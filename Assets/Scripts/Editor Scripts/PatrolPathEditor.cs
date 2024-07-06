using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PatrolPath))]
public class PatrolPathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        
        if (GUILayout.Button("Create Path Point"))
        {
            PatrolPath patrolPath = (PatrolPath)target;

            Undo.RecordObject(patrolPath, "Create Patrol Point");
            if (patrolPath.patrolPoints.Count > 1)
            {
                Vector3 lastPoint = patrolPath.patrolPoints[patrolPath.patrolPoints.Count - 1];
                patrolPath.patrolPoints.Add(lastPoint);
            } else
            {
                Vector3 newPoint = Vector3.zero;
                newPoint.z = -1;
                patrolPath.patrolPoints.Add(newPoint);
            }
            EditorUtility.SetDirty(patrolPath); // Mark the object as dirty
        }

        if (GUILayout.Button("Remove Patrol Point"))
        {
            PatrolPath patrolPath = (PatrolPath)target;

            Undo.RecordObject(patrolPath, "Remove Last Patrol Point");
            if (patrolPath.patrolPoints.Count > 1)
            {
                Vector3 lastPoint = patrolPath.patrolPoints[patrolPath.patrolPoints.Count - 1];
                patrolPath.patrolPoints.Remove(lastPoint);
            }
            EditorUtility.SetDirty(patrolPath); // Mark the object as dirty
        }
    }

    private void OnSceneGUI()
    {
        PatrolPath patrolPath = (PatrolPath)target;
        Handles.color = patrolPath.pathColor;

        for(int i = 0; i < patrolPath.patrolPoints.Count; i++)
        {

            Vector3 newPosition = Handles.FreeMoveHandle(
                patrolPath.patrolPoints[i], 
                Quaternion.identity, 
                patrolPath.sphereRadius, 
                Vector3.zero, 
                Handles.SphereHandleCap
            );


            if (patrolPath.patrolPoints[i] != newPosition)
            {
                Undo.RecordObject(patrolPath, "Move Patrol Point");
                patrolPath.patrolPoints[i] = newPosition;
            }

            if (i > 0)
            {
                Handles.DrawLine(patrolPath.patrolPoints[i - 1], patrolPath.patrolPoints[i]);
            }
        }
    }
}
