using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AIStateController
{
    public float movementSpeed = 2f;

    // vision parameters
    [Range(1, 360)] public float fovAngle = 30f;
    [Range(11, 30)] public float visionRange = 11f;
    [Range(1, 10)] public float attackRange = 10f;

    // TODO: Currently, these properties are public so AIStates can access them and pass the values to the next state if needed
    // It would probably be better to just pass the "Enemy" object to the next state, but then we'd have a duplicate of Enemy - one as a "AIStateController" and the other
    // as simply an "Enemy" - in each state:
    // Consider removing the "controller" property from the abstract class "AIState", and then creating a new derived class from AIState called
    // EnemyState which defines it's own controller - which can be a derivation of AIStateController with it's own public properties that it can 
    // access publicly, or getters
    // 
    public ICombatBehaviour combatBehaviour;
    public GameObject player;
    public Rigidbody2D rigidBody;

    AlertSystem alertSystem;
    
    Vector2 lookDirection;
    Vector2 currentTargetPos;

    const float RIGHT_ANGLE = 90f;

    float distanceFromPlayer;
    float angle;
    float alertTimer = 5;

    public bool hasTargetInSight = false;
    public bool isAlerted = false;

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

    new void Start()
    {
        player = GameObject.Find("Player");
        rigidBody = GetComponent<Rigidbody2D>();

        if (combatBehaviour == null)
            combatBehaviour = GetComponent<ICombatBehaviour>();

        base.currentState = new PatrolState(this, player.transform);
        base.currentState.Enter();
    }

    new void Update()
    {
        base.currentState.Execute();
    }

    // Returns true if the distance between the entity and the target is less than the vision range of the entity
    // and angle between the direction the entity is looking and the target is less than the entity's field of view
    public bool IsTargetInSight(Vector2 target)
    {
        distanceFromPlayer = Vector2.Distance(target, transform.position);
        lookDirection = target - rigidBody.position;

        float angleFromTarget = Vector2.Angle(transform.up, lookDirection);

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));

        RaycastHit2D hit = Physics2D.Raycast(transform.position, lookDirection, distanceFromPlayer, LayerMask.GetMask("Tilemap", "Physical Objects", "Doors", "Player"));
        Debug.DrawLine(rigidBody.position, currentTargetPos, Color.red);

        // fovAngle divided in two since the enemy is the angle spread across his left/right sides
        // TODO: Change this so that it checks to see if the player's collider is within fovAngle
        if (distanceFromPlayer <= visionRange && angleFromTarget <= (fovAngle / 2))
        {
            return hit.collider != null && hit.collider.name == "Player";
        }

        return false;
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
}
