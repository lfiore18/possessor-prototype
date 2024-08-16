using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AIStateController, IFieldOfViewUser
{
    public float movementSpeed = 2f;

    // vision parameters
    [Range(1, 360)] public float fovAngle = 30f;
    [Range(6, 15)] public float visionRange;
    [Range(1, 5)] public float attackRange;

    float distanceFromPlayer;
    float angleFromTarget;

    RaycastHit2D hit;

    // TODO: Currently, these properties are public so AIStates can access them and pass the values to the next state if needed
    public ICombatBehaviour combatBehaviour;
    public GameObject player;
    public Rigidbody2D rigidBody;

    ContactFilter2D contactFilter = new ContactFilter2D();

    AlertSystem alertSystem;
    
    Vector2 lookDirection;
    Vector2 currentTargetPos;

    public bool hasTargetInSight = false;
    public bool isAlerted = false;

    // DEBUG PURPOSES ONLY
    public List<Vector3Int> pathToTarget;

    // Each enemy has the following states:
    // 1) Patrolling - the enemy will perform a "patrolling" behaviour - walking a route, rotating to use FOV to spot player, etc
    // 2) Combat - the enemy will perform a "combat" behaviour - firing, striking, etc - until the enemy or player is dead or the enemy has lost sight of the player
    // 3) Following - when the player breaks LOS, the enemy will try to re-establish LOS by following a path back to the player by following player to "Last known position"

    // "Last known position" is defined by the following - the furthest tile in the enemy's sight range 

    // States:
    // 1) Patrolling
    //      - entry: Nothing 
    //      - default: Move to the next waypoint in the patrol route, looking for target - if the target falls in the line of sight, execute changestate
    //      - exit: Nothing
    // 2) Attacking:
    //      - entry: Nothing
    //      - default: Start performing combat action - shooting, striking, whatever it may be
    //      - exit: Nothing
    // 3) Chasing:
    //      - entry: Nothing
    //      - default: Follow a path using BFS until target is back in line of sight
    //      - exit: Nothing

    // Alert System:
    // When the enemy spots the target:
    // 1) Enemy should notifify alert system that target was spotted
    // 2) Alert system should raise an alarm, alerting all other enemies to the position of the player
    // Other enemies should



    new void Start()
    {
        player = GameObject.Find("Player");
        rigidBody = GetComponent<Rigidbody2D>();

        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        if (combatBehaviour == null) combatBehaviour = GetComponent<ICombatBehaviour>();

        base.currentState = new PatrolState(this, player.transform);
        base.currentState.Enter();
    }

    new void Update()
    {
        base.currentState.Execute();
    }

    // Returns true if the distance between the entity and the target is less than the vision range of the entity
    // and angle between the direction the entity is looking and the target is less than the entity's field of view
    public bool IsTargetInSight()
    {
        // fovAngle divided in two since the enemy is the angle spread across his left/right sides
        // TODO: Change this so that it checks to see if the player's collider is within fovAngle
        UpdateSenseData(player.transform.position);
        if (IsTargetInSightRange(visionRange))
            return RayHitTarget();
        return false;
    }

    public bool IsTargetInAttackRange()
    {
        return IsTargetInSightRange(attackRange);
    }

    bool IsTargetInSightRange(float range)
    {
        return distanceFromPlayer <= range && angleFromTarget <= (fovAngle / 2);
    }

    public bool RayHitTarget()
    {
        RayCastToTarget();
        return hit.collider != null && hit.collider.name == "Player";
    }

    void RayCastToTarget()
    {
        hit = Physics2D.Raycast(transform.position, lookDirection, distanceFromPlayer, LayerMask.GetMask("Tilemap", "Physical Objects", "Doors", "Player"));
        Debug.DrawLine(transform.position, player.transform.position, Color.red);
    }

    public void UpdateSenseData(Vector2 target)
    {
        distanceFromPlayer = Vector2.Distance(target, transform.position);
        lookDirection = target - rigidBody.position;
        angleFromTarget = Vector2.Angle(transform.up, lookDirection);
    }

    public void Aim(Vector2 target)
    {
        // Find the direction to look in by subtracting the current position of this game object from the target position in world co-ordinates
        rigidBody.rotation = Utils.RotatationAngleToTarget(rigidBody.position, target); // Returns the angle between "x" and "x, y = 1"
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
        var halfCellSize = new Vector3(.5f, .5f, 0);
        if (pathToTarget != null)
        {
            foreach (Vector3Int position in pathToTarget)
            {
                Gizmos.color = new Color(1, 0.6f, 1, 0.4f);
                Gizmos.DrawCube(position + halfCellSize, halfCellSize * 2);
            }
        }
    }
}
