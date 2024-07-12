using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public List<Vector3> patrolPoints;
    public bool lookInMovingDirection = false;
    public bool loopPatrol = false;
    
    [Header("Editor-Only Properties")]
    public Color pathColor = Color.green;
    public float sphereRadius = 0.5f;
}
