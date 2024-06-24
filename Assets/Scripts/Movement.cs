using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] Gun gun;

    Rigidbody2D rigidBody;

    // TODO: Consider removing reference to crosshair, instead give crosshair a setter
    Vector2 crosshairPosition;

    Vector2 lookDirection;
    Crosshair crosshair;

    const float RIGHT_ANGLE = 90f;
    float deltaX = 0;
    float deltaY = 0;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        crosshair = FindObjectOfType<Crosshair>();
    }


    void Update()
    {
        Aim();
        Shoot();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        deltaX = rigidBody.position.x + (Input.GetAxis("Horizontal") * Time.fixedDeltaTime * speed);
        deltaY = rigidBody.position.y + (Input.GetAxis("Vertical") * Time.fixedDeltaTime * speed);

        rigidBody.position = new Vector2(deltaX, deltaY);
    }

    private void Aim()
    {
        // Find the direction to look in by subtracting the current position of this game object from the mousePosition in world co-ordinates
        crosshairPosition = crosshair.transform.position;
        lookDirection = crosshairPosition - rigidBody.position;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - RIGHT_ANGLE; // Returns the angle between "x" and "x, y = 1"

        // Set the predetermined angle directly to the rigidbody.rotation (which is represented by a float)
        // instead of applying Rotate() to the transform every frame
        rigidBody.rotation = angle;
    }

    private void Shoot()
    {
        if (gun != null)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                gun.Fire();
            }

            if (Input.GetButtonUp("Fire1"))
            {
                gun.StopFiring();
            }
        }
    }

    public float GetDeltaX()
    {
        return deltaX;
    }

    public float GetDeltaY()
    {
        return deltaY;
    }
}
