using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : EnemyState
{
    Pathfinding pathfinding;
    List<Vector3Int> pathToTarget = new List<Vector3Int>();
    Transform targetTransform;

    float alertTime = 10;

    public ChaseState(Enemy controller, Transform targetTransform) : base(controller) 
    {
        this.pathfinding = controller.GetPathfinder();
        this.targetTransform = targetTransform;
        this.pathToTarget = pathfinding.CalculatePath(targetTransform.position);
    }

    public override void Enter()
    {
        Debug.Log("Entering Chase State");
    }

    public override void Execute()
    {
        Debug.Log("Pursuing Target");



        if (pathfinding.PlayerPositionHasChanged()) pathToTarget = pathfinding.CalculatePath(controller.transform.position);
        
        if (controller.IsTargetInSight(targetTransform.position)) controller.ChangeState(new AttackState(controller, targetTransform));
        

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
