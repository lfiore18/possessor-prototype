using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyState
{
    bool reversePath = false;
    int pathIndex = 0;

    float waitForSecs = 0;

    PatrolPath patrolPath;
    Transform targetTransform;

    public PatrolState(Enemy controller, Transform targetTransform) : base(controller) 
    {
        this.patrolPath = controller.GetComponent<PatrolPath>();
        this.targetTransform = targetTransform;
    }

    public override void Enter()
    {
        Debug.Log("Entering Patrol State");
    }

    public override void Execute()
    {
        waitForSecs -= Time.deltaTime;

        if (controller.IsTargetInSight(targetTransform.position)) controller.ChangeState(
            new ChaseState(controller, targetTransform));

        if (waitForSecs <= 0) waitForSecs = ReachedWaypointThisFrame(3) ? patrolPath.waitForSecs : 0;
    }

    public override void Exit()
    {
        Debug.Log("Exiting Patrol State");
    }

    bool ReachedWaypointThisFrame(float movementSpeed)
    {
        int endPathIndex = reversePath ? 0 : patrolPath.patrolPoints.Count - 1;
        int indexOffset = reversePath ? -1 : 1;
        var currentTargetPosition = patrolPath.patrolPoints[pathIndex];

        // Move
        FollowPath(currentTargetPosition, movementSpeed);

        // If the entity reached the end of the path, reverse the path if it's a looping patrol
        // return false so entity doesn't wait double the time
        if (Vector2.Distance(patrolPath.patrolPoints[endPathIndex], controller.transform.position) < 0.1f)
        {
            reversePath = patrolPath.loopPatrol ? !reversePath : reversePath;
            return false;
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

        if (patrolPath.lookInMovingDirection)
            controller.rigidBody.rotation = Utils.RotatationAngleToTarget(controller.transform.position, patrolPath.patrolPoints[pathIndex]);

        controller.rigidBody.position = Vector2.MoveTowards
            (controller.transform.position, new Vector2(targetPosition.x, targetPosition.y), movementThisFrame);
    }













}
