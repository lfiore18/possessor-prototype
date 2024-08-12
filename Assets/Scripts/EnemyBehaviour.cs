using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyBehaviour : MonoBehaviour, IFieldOfViewUser
{
    [SerializeField] float movementSpeed = 2f;

    // vision parameters
    [SerializeField] [Range(1, 360)] float fovAngle = 30f;
    [SerializeField] [Range(1, 30)] float visionRange = 10f;

    [SerializeField] Pathfinding pathfinding;
    [SerializeField] PatrolPath patrolPath;
    ICombatBehaviour combatBehaviour;

    bool reversePath = false;
    [SerializeField] bool patrolRoutineIsRunning = false;

    int pathIndex = 0;
    Coroutine patrolCR;
    
    List<Vector3Int> pathToPlayer = new List<Vector3Int>();
    
    GameObject player;
    AlertSystem alertSystem;
    Rigidbody2D rigidBody;

    Vector2 lookDirection;
    Vector2 currentTargetPos;

    const float RIGHT_ANGLE = 90f;

    float distanceFromPlayer;
    float angle;
    float alertTimer = 5;

    [SerializeField] bool hasTargetInSight = false;
    [SerializeField] bool isAlerted = false;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        rigidBody = GetComponent<Rigidbody2D>();

        alertSystem = FindObjectOfType<AlertSystem>();

        AddEventListeners();

        if (pathfinding == null)
            pathfinding = FindObjectOfType<Pathfinding>();

        if (patrolPath == null)
            patrolPath = GetComponent<PatrolPath>();

        if (combatBehaviour == null)
            combatBehaviour = GetComponent<ICombatBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTargetPos = player.transform.position;
        Search(currentTargetPos);

        if (isAlerted)
        {
            // Stop the patrol coroutine
            if (patrolRoutineIsRunning)
            {
                StopCoroutine(patrolCR);
                patrolRoutineIsRunning = false;
            }

            if (IsTargetInLOS(currentTargetPos))
            {
                Aim(currentTargetPos);
                combatBehaviour.PerformCombatAction();

                alertTimer = 5;
                Debug.Log("Still has sight");
            } 
            else
            {
                // Runs when the target has broken line of sight with the enemy

                // Stop attacking because target is no longer in sight
                combatBehaviour.StopCombatAction();

                // TODO: implement a "last known position" mechanic
                // the enemy can continue to track the target for a set period of time (say 5 seconds)
                // and will explore the last recorded position in pathfinding algorithm
                if (pathfinding.PlayerPositionHasChanged())
                    pathToPlayer = pathfinding.CalculatePath(this.transform.position);

                // If the target has broken line of sight with the enemy, start the alarm timer
                if (!alertSystem.GetAlarmStatus())
                    alertSystem.StartAlarmTimer();

                // If the enemy reaches the path without having reached the player, enemy is no longer alerted
                // this accounts for if the player has reached an area that is currently inaccessible
                if (pathToPlayer.Count > 0)
                    FollowPlayer();

                alertTimer -= Time.deltaTime;
                Debug.Log("Lost sight for: " + alertTimer + " seconds");
            }

            // If the enemy reaches the path without having reached the player, enemy is no longer alerted
            // this accounts for if the player has reached an area that is currently inaccessible        
            if (alertTimer <= 0)
                isAlerted = false;
        } 
        else
        {
            if (!patrolRoutineIsRunning && patrolPath != null)
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
        if (IsTargetInLOS(target))
        {
            TargetSighted();
            AlarmStartedHandler();
            GetComponentInChildren<fieldOfView>().SetColour(Color.red);
        }
        else
        {
            TargetLost();
            GetComponentInChildren<fieldOfView>().SetColour(Color.grey);
        }
    }

    // Returns true if the distance between the entity and the target is less than the vision range of the entity
    // and angle between the direction the entity is looking and the target is less than the entity's field of view
    bool IsTargetInLOS(Vector2 target)
    {
        distanceFromPlayer = Vector2.Distance(target, transform.position);
        lookDirection = target - rigidBody.position;

        float angleFromPlayer = Vector2.Angle(transform.up, lookDirection);

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, distanceFromPlayer, LayerMask.GetMask("Tilemap", "Physical Objects", "Doors", "Player"));
        Debug.DrawLine(rigidBody.position, currentTargetPos, Color.red);

        // fovAngle divided in two since the enemy is the angle spread across his left/right sides
        // TODO: Change this so that it checks to see if the player's collider is within fovAngle
        if (distanceFromPlayer <= visionRange && angleFromPlayer <= (fovAngle / 2))
        {
            return hit.collider != null && hit.collider.name == "Player";
        }

        return false;
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

            Aim(pos);

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

    public void AlarmStartedHandler()
    {
        isAlerted = true;

        pathToPlayer = pathfinding.CalculatePath(this.transform.position);

        if (!alertSystem.GetAlarmStatus())   
            alertSystem.RaiseAlarm();

        alertSystem.UpdateLastKnownPosition(currentTargetPos);

        //Debug.Log(gameObject.name + " alerted");
    }

    public void AlarmExpiredHandler()
    {
        isAlerted = false;
    }


    public float GetFovAngle()
    {
        return fovAngle;
    }

    public float GetVisionRange()
    {
        return visionRange;
    }

    public void Kill(Vector2 hitPoint)
    {
        GetComponent<Death>().Kill(hitPoint);

        RemoveEventListeners();

        Destroy(GetComponent<Movement>());
        Destroy(gameObject.GetComponentInChildren<fieldOfView>().gameObject);
        Destroy(this);
    }

    void AddEventListeners()
    {
        // Start listening to alert system events
        alertSystem.onAlarmStarted.AddListener(AlarmStartedHandler);
        alertSystem.onAlarmTimerExpired.AddListener(AlarmExpiredHandler);
    }

    void RemoveEventListeners()
    {
        // Remove alarm event listeners
        alertSystem.onAlarmStarted.RemoveListener(AlarmStartedHandler);
        alertSystem.onAlarmTimerExpired.AddListener(AlarmExpiredHandler);
        TargetLost();
    }

    // Update the rest of the enemy network
    void TargetSighted()
    {
        alertSystem.AddSelfToNetwork(this);
        hasTargetInSight = true;
    }

    void TargetLost()
    {
        alertSystem.RemoveSelfFromNetwork(this);
        hasTargetInSight = false;
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
