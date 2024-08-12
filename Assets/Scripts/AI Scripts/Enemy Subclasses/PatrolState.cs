using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : AIState
{
    bool reversePath = false;
    int pathIndex = 0;
    float waitForSecs = 5;

    PatrolPath entityPatrolPath;
    Rigidbody2D rigidBody;

    public PatrolState(AIStateController controller, PatrolPath patrolPath, Rigidbody2D rigidBody2D) : base(controller) 
    {
        entityPatrolPath = patrolPath;
        rigidBody = rigidBody2D;
    }

    public override void Enter()
    {
        Enemy enemy = controller as Enemy;

        entityPatrolPath = enemy.patrolPath;
        Debug.Log("Entering Patrol State");
    }

    public override void Execute()
    {
        Enemy enemy = controller as Enemy;

        waitForSecs -= Time.deltaTime;

        if (waitForSecs <= 0)
        {
            Debug.Log("Waited " + 5 + " seconds");
            waitForSecs = ReachedWaypointThisFrame(3) ? 5 : 0;          
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }

    bool ReachedWaypointThisFrame(float movementSpeed)
    {
        int endPathIndex = reversePath ? 0 : entityPatrolPath.patrolPoints.Count - 1;
        int indexOffset = reversePath ? -1 : 1;
        var currentTargetPosition = entityPatrolPath.patrolPoints[pathIndex];

        FollowPath(currentTargetPosition, movementSpeed);

        // If the entity reached the end of the path
        if (Vector2.Distance(entityPatrolPath.patrolPoints[endPathIndex], controller.transform.position) < 0.1f)
        {
            reversePath = entityPatrolPath.loopPatrol ? !reversePath : reversePath;
            return true;
        }

        if (Vector2.Distance(controller.transform.position, currentTargetPosition) < 0.1f)
        {
            pathIndex += indexOffset;
            return true;
        }

        return false;
    }


    void FollowPath(Vector3 targetPosition, float movementSpeed)
    {
        var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

        if (entityPatrolPath.lookInMovingDirection)
            rigidBody.rotation = Utils.RotatationAngleToTarget(controller.transform.position, entityPatrolPath.patrolPoints[pathIndex]);

        rigidBody.position = Vector2.MoveTowards
            (controller.transform.position, new Vector2(targetPosition.x, targetPosition.y), movementThisFrame);
    }













}
