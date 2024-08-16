using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : EnemyState
{
    Pathfinding pathfinding;
    List<Vector3Int> pathToTarget = new List<Vector3Int>();
    Transform targetTransform;

    float alertTime = 2;
    float secsLeftToFindTarget = 0;

    public ChaseState(Enemy controller, Transform targetTransform) : base(controller) 
    {
        this.pathfinding = controller.GetPathfinder();
        this.targetTransform = targetTransform;
        this.pathToTarget = pathfinding.CalculatePath(controller.transform.position);
    }

    public override void Enter()
    {
        secsLeftToFindTarget = alertTime;
        Debug.Log("Entering Chase State");
    }

    public override void Execute()
    {
        Debug.Log("Pursuing Target");
        // NOTE: UNCOMMENT FOR DEBUG GIZMOS        
        controller.pathToTarget.Clear();
        controller.pathToTarget.AddRange(pathToTarget);

        bool targetInSight = controller.IsTargetInSight();
        
        // Only count down if enemy is out of sight
        if (!targetInSight)
        {
            secsLeftToFindTarget -= Time.deltaTime;
            Debug.Log("Seconds left to find target: " + secsLeftToFindTarget);
        }

        // Only track path to target if target is in sight or has been out of sight for less than "secsLeftToFindTarget"
        if (targetInSight || secsLeftToFindTarget > 0) 
            pathToTarget = pathfinding.CalculatePath(controller.transform.position);

        // If target is in sight and in attack range, attack
        if (targetInSight)
        {
            if (controller.IsTargetInAttackRange())
                controller.ChangeState(new AttackState(controller, targetTransform));
            secsLeftToFindTarget = alertTime;
        }

        // If enemy has reached the end of the path and run out of time to track target, switch to patrol state
        if (pathToTarget.Count <= 0 && secsLeftToFindTarget <= 0)
            controller.ChangeState(new PatrolState(controller, targetTransform));

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
