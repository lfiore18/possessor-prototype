using UnityEngine;
using System.Collections;

public class fieldOfView : MonoBehaviour
{
    [SerializeField] [Range(1, 20)] int meshResolution = 3;
    [SerializeField] public Color meshAlertColor;
    EnemyBehaviour parent;

    // parameters determined by parent properties
    float angleFov; 
    float viewDistance;
    float angleStart;

    Mesh mesh;
    Renderer rend;
    Vector3[] vertices;
    int[] triangles;
    Vector2 rayCastOrigin;

    Crosshair crosshair;

    void Start()
    {
        mesh = new Mesh();
        rend = GetComponent<MeshRenderer>();
        Debug.Log(rend.sortingLayerName);

        GetComponent<MeshFilter>().mesh = mesh;
        crosshair = FindObjectOfType<Crosshair>();

        parent = gameObject.GetComponentInParent<EnemyBehaviour>();

        if (parent != null)
        {
            angleFov = parent.GetFovAngle();
            viewDistance = parent.GetVisionRange();
        }

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
        angleStart = 90 - (angleFov / 2);

        vertices = new Vector3[3 + (meshResolution - 1)]; // we start with 3, the minimum number of vertices

        float angleStep = angleFov / meshResolution;
        float angleCurr = angleStart;

        vertices[0] = rayCastOrigin;

        RaycastHit2D hit;
        for (int i = 1; i < vertices.Length - 1; i++)
        {
            hit = Physics2D.Raycast(transform.parent.position, AngleToVector(angleCurr), viewDistance, LayerMask.GetMask("Tilemap, Physical Objects", "Player"));
            Vector2 vertex = (Vector2)AngleToVector(angleCurr) * (hit.collider != null ? hit.distance : viewDistance);
            vertices[i] = vertex;

            angleCurr += angleStep;
        }
        hit = Physics2D.Raycast(transform.parent.position, AngleToVector(angleCurr), viewDistance, LayerMask.GetMask("Tilemap, Physical Objects", "Player"));
        vertices[vertices.Length - 1] = AngleToVector(angleStart + angleFov) * (hit.collider != null ? hit.distance : viewDistance);

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
        int rayCount = meshResolution - 1;

        float angleStep = angleFov / meshResolution;
        float angleCurr = 0;

        //Debug.Log("ray count = " +  rayCount);
        RaycastHit2D hit;
        for (; angleCurr < angleFov;)
        {
            hit = Physics2D.Raycast(transform.parent.position, AngleToVector(angleCurr), viewDistance, LayerMask.GetMask("Physical Objects"));
            if (hit.collider != null)
            {
                Gizmos.DrawRay(transform.position, AngleToVector(angleCurr) * hit.distance);
            } else
            {
                Gizmos.DrawRay(transform.position, AngleToVector(angleCurr) * viewDistance);
            }

            angleCurr += angleStep;
        }

        hit = Physics2D.Raycast(transform.parent.position, AngleToVector(angleFov), viewDistance, LayerMask.GetMask("Physical Objects"));
        Gizmos.DrawRay(transform.position, AngleToVector(angleFov) * (hit.collider != null ? hit.distance : viewDistance ));
    }


}