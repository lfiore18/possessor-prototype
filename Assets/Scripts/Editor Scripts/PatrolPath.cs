using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public List<Vector3> patrolPoints = new List<Vector3>();
    public bool lookInMovingDirection = false;
    
    [Header("Editor-Only Properties")]
    public Color pathColor = Color.green;
    public float sphereRadius = 0.5f;
}
