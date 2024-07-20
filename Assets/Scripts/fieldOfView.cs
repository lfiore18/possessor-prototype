using UnityEngine;
using System.Collections;

public interface IFieldOfView
{
    float GetFovAngle();
    float GetVisionRange();
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class fieldOfView : MonoBehaviour
{
    [SerializeField] [Range(1, 20)] int meshResolution = 3;
    [SerializeField] public Color meshAlertColor;
    IFieldOfView parent;

    // parameters determined by parent properties
    float angleFov; 
    float viewDistance;
    float angleStart;

    Mesh mesh;
    Renderer rend;
    Vector3[] vertices;
    int[] triangles;
    Vector2 rayCastOrigin;

    void Start()
    {
        mesh = new Mesh();
        rend = GetComponent<MeshRenderer>();
        Debug.Log(rend.sortingLayerName);

        GetComponent<MeshFilter>().mesh = mesh;

        parent = gameObject.GetComponentInParent<IFieldOfView>();

        if (parent != null)
        {
            angleFov = parent.GetFovAngle();
            viewDistance = parent.GetVisionRange();
        }

        Debug.Log(parent + " fieldOfView: " + angleFov);

        CreateShape();
        UpdateMesh();
    }

    void Update()
    {
        if (parent != null)
        {
            angleFov = parent.GetFovAngle();
            viewDistance = parent.GetVisionRange();
        }

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        // We need at least 3 vertices to draw a mesh representing our enemy's "field of view"

        // 1st vertex will be at transform of the enemy. We can assume it is 0, 0 for now
        // 2nd will be "viewRange" distance away from the 1st
        // 3rd will be "viewRange" distance away from the 1st, at an angle equal to "angle_fov" away from the 2nd

        // To draw a mesh that more accurately represents the enemy's field of view, we need to draw a cone
        // We'll place more vertices between the 2nd and 3rd vertices to do this, creating a higher resolution mesh

        // The number of vertices = 3 + (meshResolution - 1)
        // We subtract 1 from meshResolution to account for the fact that we've added 3 vertices already -
        // for the rayCastOrigin, start and end of the angle

        // The smoothness of the arc will be determined by the number of steps in between vertices 2 and 3
        // For each step, a ray will be fired at "viewRange" distance away from the 1st vertex, at an angle

        rayCastOrigin = Vector2.zero;
        angleStart = 90 + transform.eulerAngles.z - (angleFov / 2);

        vertices = new Vector3[3 + (meshResolution - 1)]; // we start with 3, the minimum number of vertices

        float angleStep = angleFov / meshResolution;
        float angleCurr = angleStart;

        vertices[0] = rayCastOrigin;

        Vector2 adjustedAngle;
        Vector2 vertex;
        RaycastHit2D hit;
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            adjustedAngle = AngleToVector(angleCurr);

            hit = Physics2D.Raycast(transform.position, adjustedAngle, viewDistance, LayerMask.GetMask("Tilemap", "Physical Objects", "Player"));
            
            Debug.DrawRay(transform.position, adjustedAngle * (hit.collider != null ? hit.distance : viewDistance));
            vertex = adjustedAngle * (hit.collider != null ? hit.distance : viewDistance);
            vertices[i] = transform.InverseTransformPoint((Vector3)vertex + transform.position);
            vertices[i].z = 0;
            angleCurr += angleStep;
        }

        // Add the final vertex
        adjustedAngle = AngleToVector(90 + transform.eulerAngles.z + (angleFov / 2));

        hit = Physics2D.Raycast(transform.position, adjustedAngle, viewDistance, LayerMask.GetMask("Tilemap", "Physical Objects", "Player"));
        
        Debug.DrawRay(transform.position, adjustedAngle * (hit.collider != null ? hit.distance : viewDistance));
        vertex = adjustedAngle * (hit.collider != null ? hit.distance : viewDistance);

        vertices[vertices.Length - 1] = transform.InverseTransformPoint((Vector3)vertex + transform.position);
        vertices[vertices.Length - 1].z = 0;

        int numOfTris = meshResolution * 3;

        triangles = new int[numOfTris];

        int triangleIncrease = 0;
        for (int i = 0; i < numOfTris; i += 3)
        {
            triangles[i] = 0;
            triangles[i + 1] = 2 + triangleIncrease;
            triangles[i + 2] = 1 + triangleIncrease;
            triangleIncrease++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void SetColour(Color color)
    {
        rend.material.SetColor("_Color", color); 
    }

    Vector3 AngleToVector(float angle)
    {
        float angleInRad = angle * (Mathf.PI / 180f);

        return new Vector3(Mathf.Cos(angleInRad), Mathf.Sin(angleInRad));
    }

    private void OnDrawGizmos()
    {
        float angleStep = angleFov / meshResolution;
        float angleCurr = 0;

        Vector2 adjustedAngle;

        RaycastHit2D hit;

        float adjustedAngleCalc = transform.eulerAngles.z + 90f;

        for (; angleCurr < angleFov;)
        {
            adjustedAngle = AngleToVector(adjustedAngleCalc - (angleFov / 2) + angleCurr);
            hit = Physics2D.Raycast(transform.parent.position, adjustedAngle, viewDistance, LayerMask.GetMask("Doors", "Tilemap", "Physical Objects"));
            Gizmos.DrawRay(transform.position, adjustedAngle * (hit.collider != null ? hit.distance : viewDistance));

            angleCurr += angleStep;
        }

        adjustedAngle = AngleToVector(adjustedAngleCalc + (angleFov / 2));
        hit = Physics2D.Raycast(transform.parent.position, adjustedAngle, viewDistance, LayerMask.GetMask("Doors", "Tilemap", "Physical Objects"));
        Gizmos.DrawRay(transform.position, adjustedAngle * (hit.collider != null ? hit.distance : viewDistance));
    }


}