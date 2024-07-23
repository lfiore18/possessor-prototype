using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour, IFieldOfView
{
    [SerializeField] float movementSpeed = 2f;

    // vision parameters
    [SerializeField] [Range(1, 360)] float fovAngle = 30f;
    [SerializeField] [Range(1, 30)] float visionRange = 10f;

    [SerializeField] Pathfinding pathfinding;
    [SerializeField] PatrolPath patrolPath;

    bool reversePath = false;
    bool patrolRoutineIsRunning = false;
    int pathIndex = 0;
    Coroutine patrolCR;
    
    List<Vector3Int> pathToPlayer = new List<Vector3Int>();
    
    GameObject player;
    AlertSystem alertSystem;
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

        alertSystem = FindObjectOfType<AlertSystem>();
        alertSystem.AddListener(OnPlayerSpotted);
    }

    // Update is called once per frame
    void Update()
    {
        currentTarget = player.transform.position;
        Search(currentTarget);

        if (alerted)
        {
            //Debug.Break();

            // Stop the patrol coroutine
            if (patrolRoutineIsRunning)
            {
                StopCoroutine(patrolCR);
                patrolRoutineIsRunning = false;
            }
            
            Aim(currentTarget);

            if (pathfinding.PlayerPositionHasChanged())
                pathToPlayer = pathfinding.CalculatePath(this.transform.position);

            // If the enemy reaches the path without having reached the player, enemy is no longer alerted
            // this accounts for if the player has reached an area that is currently in accessible
            if (pathToPlayer.Count > 0)
                FollowPlayer();
            else
                alerted = false;
        } 
        else
        {
            if (!patrolRoutineIsRunning)
            {
                patrolCR = StartCoroutine(MoveAndWait());
            }
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, distanceFromPlayer, LayerMask.GetMask("Tilemap", "Physical Objects", "Doors", "Player"));
        Debug.DrawLine(rigidBody.position, currentTarget, Color.red);

        if (hit.collider != null)
        {
            //Debug.Log("Raycast Hit " + hit.collider.name);
        }

        // fovAngle divided in two since the enemy is the angle spread across his left/right sides
        // TODO: Change this so that it checks to see if the player's collider is within fovAngle
        if (distanceFromPlayer <= visionRange && angleFromPlayer <= (fovAngle / 2))
        {
            if (hit.collider != null && hit.collider.name == "Player")
            {
                OnPlayerSpotted();
                GetComponentInChildren<fieldOfView>().SetColour(Color.red);
            } 
        }
        else
        {
            GetComponentInChildren<fieldOfView>().SetColour(Color.grey);
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

        if (pathToPlayer == null)
            pathToPlayer = pathfinding.CalculatePath(this.transform.position);


        if (pathToPlayer.Count >= 1)
        {
            var targetPosition = pathToPlayer[0];
            var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

            Vector2 pos = new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f);

            // If a door has shut in the way of the path, remove the rest of the path
            // TODO: This almost certainly will need to be updated to become more robust
            if (Physics2D.OverlapPoint(pos, LayerMask.GetMask("Doors")))
            {
                pathToPlayer.RemoveRange(pathToPlayer.IndexOf(targetPosition), pathToPlayer.Count);
                return;
            }

            rigidBody.position = Vector2.MoveTowards
                (transform.position, new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f), movementThisFrame);

            if (pathfinding.GetCellPosition(transform.position) == targetPosition)            
                pathToPlayer.Remove(targetPosition);         
        }
    }

    /*    private void FollowPath()
        {
            int targetIndex = reversePath ? 0 : patrolPath.patrolPoints.Count - 1;
            int indexOffset = reversePath ? -1 : 1;

            var targetPosition = patrolPath.patrolPoints[pathIndex];
            var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

            if (patrolPath.lookInMovingDirection)
                Aim(patrolPath.patrolPoints[pathIndex]);     

            rigidBody.position = Vector2.MoveTowards
                (transform.position, new Vector2(targetPosition.x, targetPosition.y), movementThisFrame);

            if (Vector2.Distance(patrolPath.patrolPoints[targetIndex], transform.position) < 0.1f)
            {
                reversePath = patrolPath.loopPatrol ? !reversePath : reversePath;
                return;
            }

            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                pathIndex += indexOffset;
            }
        }*/

    IEnumerator MoveAndWait()
    {
        patrolRoutineIsRunning = true;

        int targetIndex = reversePath ? 0 : patrolPath.patrolPoints.Count - 1;
        int indexOffset = reversePath ? -1 : 1;
        var targetPosition = patrolPath.patrolPoints[pathIndex];

        FollowPath(targetPosition);

        if (Vector2.Distance(patrolPath.patrolPoints[targetIndex], transform.position) < 0.1f)
        {
            reversePath = patrolPath.loopPatrol ? !reversePath : reversePath;
            patrolRoutineIsRunning = false;
            yield break;   
        }

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            pathIndex += indexOffset;
            yield return new WaitForSeconds(1);
        }

        patrolRoutineIsRunning = false;
    }

    void FollowPath(Vector3 targetPosition)
    {
        var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

        if (patrolPath.lookInMovingDirection)
            Aim(patrolPath.patrolPoints[pathIndex]);

        rigidBody.position = Vector2.MoveTowards
            (transform.position, new Vector2(targetPosition.x, targetPosition.y), movementThisFrame);
    }

    public void OnPlayerSpotted()
    {
        alerted = true;

        if (pathToPlayer.Count < 1)
            pathToPlayer = pathfinding.CalculatePath(this.transform.position);

        if (!alertSystem.GetStatus())   
            alertSystem.SetAlert();

        Debug.Log(gameObject.name + " alerted");
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
