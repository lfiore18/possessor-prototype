﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AIStateController
{
    [SerializeField] float movementSpeed = 2f;

    // vision parameters
    [SerializeField] [Range(1, 360)] float fovAngle = 30f;
    [SerializeField] [Range(1, 30)] float visionRange = 10f;

    [SerializeField] Pathfinding pathfinding;
    public PatrolPath patrolPath;
    ICombatBehaviour combatBehaviour;

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

    float waitForSecs = 5f;
    float timeSinceLastUpdate = 0;

    new void Start()
    {
        player = GameObject.Find("Player");
        rigidBody = GetComponent<Rigidbody2D>();

        if (pathfinding == null)
            pathfinding = FindObjectOfType<Pathfinding>();

        if (patrolPath == null)
            patrolPath = GetComponent<PatrolPath>();

        if (combatBehaviour == null)
            combatBehaviour = GetComponent<ICombatBehaviour>();

        base.currentState = new PatrolState(this, patrolPath, rigidBody);
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

    public float Aim(Vector2 target)
    {
        // Find the direction to look in by subtracting the current position of this game object from the target position in world co-ordinates
        return Utils.RotatationAngleToTarget(rigidBody.position, target); // Returns the angle between "x" and "x, y = 1"
    }
}
