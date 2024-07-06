using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] float movementSpeed = 2f;

    // vision parameters
    [SerializeField] [Range(1, 360)] float fovAngle = 45f;
    [SerializeField] [Range(1, 30)] float visionRange = 10f;

    [SerializeField] Pathfinding pathfinding;
    [SerializeField] PatrolPath patrolPath;
    int pathIndex = 0;

    List<Vector3Int> pathToPlayer;
    

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
            //Move();
            
            if (pathfinding.PlayerPositionHasChanged())
                pathToPlayer = pathfinding.CalculatePath(this.transform.position);
            
            FollowPlayer();
        } 
        else
        {
            FollowPath();
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, distanceFromPlayer, LayerMask.GetMask("Physical Objects", "Player"));
        Debug.DrawLine(rigidBody.position, currentTarget, Color.red);

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

    private void FollowPlayer()
    {
        // Follow the calculated path to the player
        // TODO: Remove pathIndex, it's never used
        if (pathToPlayer.Count >= 1)
        {
            var targetPosition = pathToPlayer[0];
            var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

            rigidBody.position = Vector2.MoveTowards
                (transform.position, new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f), movementThisFrame);

            if (pathfinding.GetCellPosition(transform.position) == targetPosition)            
                pathToPlayer.Remove(targetPosition);         
        }
    }

    private void FollowPath()
    {
        //TODO: Fill this in

        if (pathIndex <= patrolPath.patrolPoints.Count - 1)
        {
            var targetPosition = patrolPath.patrolPoints[pathIndex];
            var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

            if (patrolPath.lookInMovingDirection)
            {
                Aim(patrolPath.patrolPoints[pathIndex]);
            }

            rigidBody.position = Vector2.MoveTowards
                (transform.position, new Vector2(targetPosition.x, targetPosition.y), movementThisFrame);

            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                pathIndex++;
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

    private void OnDrawGizmos()
    {
        var halfCellSize = pathfinding.halfCellSize;
        if (pathToPlayer != null)
        {
            foreach (Vector3Int position in pathToPlayer)
            {
                Gizmos.color = new Color(1, 0.6f, 1, 0.4f);
                Gizmos.DrawCube(position + halfCellSize, halfCellSize*2);
            }
        }
    }
}
