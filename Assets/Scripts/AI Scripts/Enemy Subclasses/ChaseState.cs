using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : EnemyState
{
    Pathfinding pathfinding;
    List<Vector3Int> pathToTarget = new List<Vector3Int>();
    Transform targetTransform;

    float alertTime = 1;
    float secsLeftToFindTarget = 0;

    public ChaseState(Enemy controller, Transform targetTransform) : base(controller) 
    {
        this.pathfinding = controller.GetPathfinder();
        this.targetTransform = targetTransform;
        this.pathToTarget = pathfinding.CalculatePath(targetTransform.position);
    }

    public override void Enter()
    {
        secsLeftToFindTarget = alertTime;
        Debug.Log("Entering Chase State");
    }

    public override void Execute()
    {
        Debug.Log("Pursuing Target");

        bool targetInSight = controller.IsTargetInSight();
        if (!targetInSight)
        {
            secsLeftToFindTarget -= Time.deltaTime;
            Debug.Log("Secs left: " + secsLeftToFindTarget);
        }

        // Only track path to target if target is in sight or has been out of sight for less than "secsLeftToFindTarget"
        if (pathfinding.PlayerPositionHasChanged() && (targetInSight || secsLeftToFindTarget > 0)) 
            pathToTarget = pathfinding.CalculatePath(controller.transform.position);

        // NOTE: UNCOMMENT FOR DEBUG GIZMOS        
        controller.pathToTarget.Clear();
        controller.pathToTarget.AddRange(pathToTarget);

        // If target is in sight and in attack range, attack
        if (targetInSight)
        {
            if (controller.IsTargetInAttackRange())
                controller.ChangeState(new AttackState(controller, targetTransform));
            secsLeftToFindTarget = alertTime;
        }

        if (pathToTarget.Count <= 0 && secsLeftToFindTarget <= 0)
            controller.ChangeState(new PatrolState(controller, targetTransform));


        // If player is still in sight but out of attack range
            // continue to follow
        // Continue to track the player for 1 second
        // If 1 second timer has expired
            // do not update path to target
            // follow path to target until last index
                // if player is not in sight at last index
                    // return to patrol

        FollowTarget();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Chase State");
    }

    private void FollowTarget()
    {
        // Follow the calculated path to the player

        if (pathToTarget == null)
            pathToTarget = pathfinding.CalculatePath(controller.transform.position);


        if (pathToTarget.Count >= 1)
        {
            var targetPosition = pathToTarget[0];
            var movementThisFrame = (controller.movementSpeed * 3) * Time.fixedDeltaTime;

            Vector2 pos = new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f);

            controller.Aim(pos);
            // enemy.Aim(enemy.IsTargetInSight(targetTransform.position) ? targetTransform.position : targetPosition);          

            // If a door has shut in the way of the path, remove the rest of the path
            // TODO: This almost certainly will need to be updated to become more robust
            if (Physics2D.OverlapPoint(pos, LayerMask.GetMask("Doors")))
            {
                pathToTarget.RemoveRange(pathToTarget.IndexOf(targetPosition), pathToTarget.Count);
                return;
            }

            controller.rigidBody.position = Vector2.MoveTowards
                (controller.transform.position, new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f), movementThisFrame);

            if (pathfinding.GetCellPosition(controller.transform.position) == targetPosition)
                pathToTarget.Remove(targetPosition);
        }
    }
}
