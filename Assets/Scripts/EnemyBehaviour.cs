using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] float movementSpeed = 2f;

    // vision parameters
    [SerializeField] [Range(1, 360)] float fovAngle = 45f;
    [SerializeField] [Range(1, 30)] float visionRange = 10f;

    GameObject player;

    Rigidbody2D rigidBody;

    Vector2 lookDirection;
    Vector2 currentTarget;

    const float RIGHT_ANGLE = 90f;

    float distanceFromPlayer;
    float angle;

    bool alerted = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTarget = player.transform.position;
        Search(currentTarget);

        if (alerted)
        {
            Aim(currentTarget);
            Move();
        }
    }

    private void Search(Vector2 target)
    {
        // If the distance between the player and the enemy is less than the vision range of the enemy
        // And angle between the direction the enemy is looking and the player is less than vision cone angle
        // Enemy alerted!

        distanceFromPlayer = Vector2.Distance(target, transform.position);
        lookDirection = target - rigidBody.position;

        float angleFromPlayer = Vector2.Angle(transform.up, lookDirection);

        ContactFilter2D contactFilter = new ContactFilter2D();

        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentTarget, distanceFromPlayer, LayerMask.GetMask("Physical Objects", "Player"));
        Debug.DrawLine(rigidBody.position, currentTarget, Color.red);
        //Debug.DrawLine(rigidBody.position, hit.point);

        if (hit.collider != null)
        {
            //Debug.Log("Raycast Hit " + hit.collider.name);
        }

        if (distanceFromPlayer <= visionRange && angleFromPlayer <= fovAngle)
        {
            if (hit.collider != null && hit.collider.name == "Player")
            {
                //Debug.Log("Sighted!");
                alerted = true;

                GetComponentInChildren<fieldOfView>().SetColour();
            }
        }
    }

    private void Aim(Vector2 target)
    {
        // Find the direction to look in by subtracting the current position of this game object from the target position in world co-ordinates
        lookDirection = target - rigidBody.position;

        angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - RIGHT_ANGLE; // Returns the angle between "x" and "x, y = 1"

        // Set the predetermined angle directly to the rigidbody.rotation (which is represented by a float)
        // instead of applying Rotate() to the transform every frame
        rigidBody.rotation = angle;
    }

    private void Move()
    {
        float deltaX = rigidBody.position.x + (Time.fixedDeltaTime * lookDirection.x * movementSpeed);
        float deltaY = rigidBody.position.y + (Time.fixedDeltaTime * lookDirection.y * movementSpeed);

        rigidBody.position = new Vector2(deltaX, deltaY);
    }

    public float GetFovAngle()
    {
        return fovAngle;
    }

    public float GetVisionRange()
    {
        return visionRange;
    }
}
