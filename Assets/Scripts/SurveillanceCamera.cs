using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurveillanceCamera : MonoBehaviour, IFieldOfView
{
    // vision parameters
    [Header("Vision Parameters")]
    [SerializeField] [Range(1, 360)] float fovAngle = 30f;
    [SerializeField] [Range(1, 30)] float visionRange = 10f;

    [Header("Camera Behaviour Settings")]
    [Tooltip("Angle values are relative to camera's starting position which is North")]
    [SerializeField] [Range(0, 359)]float rotateDegreesFromStart;
    [SerializeField] float movementSpeed = 2f;

    GameObject player;
    Vector2 currentTarget;
    Vector2 lookDirection;

    Rigidbody2D rb2D;

    const float RIGHT_ANGLE = 90f;

    float distanceFromPlayer;
    bool alerted = false;

    float degreesLeftToTurn;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        degreesLeftToTurn = rotateDegreesFromStart;
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTarget = player.transform.position;
        Search(currentTarget);

        if (alerted)
        {
            Aim(currentTarget);
        }
        else
        {
            Turn();
        }
    }

    void Turn()
    {
        // remove from degrees left to turn, the absolute value of the movespeed (else it'll add to degreesleft rather than subtract when we flip the value)
        degreesLeftToTurn -= Time.deltaTime * Mathf.Abs(movementSpeed);
        movementSpeed = degreesLeftToTurn <= 0 ? -movementSpeed : movementSpeed;

        // Change camera's rotation every frame until it reaches target rotation
        rb2D.rotation += movementSpeed * Time.deltaTime;

        degreesLeftToTurn = degreesLeftToTurn <= 0 ? rotateDegreesFromStart : degreesLeftToTurn;
    }

    private void Aim(Vector2 target)
    {
        // Find the direction to look in by subtracting the current position of this game object from the target position in world co-ordinates
        Vector2 lookDirection = target - rb2D.position;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - RIGHT_ANGLE; // Returns the angle between "x" and "x, y = 1"

        // Set the predetermined angle directly to the rigidbody.rotation (which is represented by a float)
        // instead of applying Rotate() to the transform every frame
        rb2D.rotation = angle;
    }

    private void Search(Vector2 target)
    {
        // If the distance between the player and the enemy is less than the vision range of the enemy
        // And angle between the direction the enemy is looking and the player is less than vision cone angle
        // Enemy alerted!
        distanceFromPlayer = Vector2.Distance(target, transform.position);
        lookDirection = target - rb2D.position;

        float angleFromPlayer = Vector2.Angle(transform.up, lookDirection);

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, distanceFromPlayer, LayerMask.GetMask("Tilemap", "Physical Objects", "Doors", "Player"));
        Debug.DrawRay(transform.position, lookDirection, Color.red);


        // fovAngle divided in two since the enemy is the angle spread across his left/right sides
        // TODO: Change this so that it checks to see if the player's whole collider is within fovAngle
        if (distanceFromPlayer <= visionRange && angleFromPlayer <= (fovAngle / 2))
        {
            if (hit.collider != null && hit.collider.name == "Player")
            {
                alerted = true;
                GetComponentInChildren<fieldOfView>().SetColour(Color.red);
            }
        }
        else
        {
            GetComponentInChildren<fieldOfView>().SetColour(Color.grey);
        }
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
