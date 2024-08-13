using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : AIState
{
    Pathfinding pathfinding;
    List<Vector3Int> pathToTarget = new List<Vector3Int>();
    Transform targetTransform;
    Rigidbody2D rigidBody;

    float movementSpeed;

    public ChaseState(AIStateController controller, Rigidbody2D rigidBody, Pathfinding pathfinding, Transform targetTransform, float movementSpeed) : base(controller) 
    {
        this.rigidBody = rigidBody;
        this.pathfinding = controller.GetPathfinder();
        this.targetTransform = targetTransform;
        this.pathToTarget = pathfinding.CalculatePath(targetTransform.position);
        this.movementSpeed = movementSpeed;
    }

    public override void Enter()
    {
        Debug.Log("Entering Chase State");
    }

    public override void Execute()
    {
        Debug.Log("Pursuing Target");
        Enemy enemy = controller as Enemy;

        if (pathfinding.PlayerPositionHasChanged()) pathToTarget = pathfinding.CalculatePath(controller.transform.position);
        
        if (enemy.IsTargetInSight(targetTransform.position)) controller.ChangeState(new AttackState(controller, ));
        
        FollowTarget();
    }

    public override void Exit()
    {
        Debug.Log("Exiting Chase State");
    }

    private void FollowTarget()
    {
        // Follow the calculated path to the player

        Enemy enemy = controller as Enemy;

        if (pathToTarget == null)
            pathToTarget = pathfinding.CalculatePath(enemy.transform.position);


        if (pathToTarget.Count >= 1)
        {
            var targetPosition = pathToTarget[0];
            var movementThisFrame = movementSpeed * Time.fixedDeltaTime;

            Vector2 pos = new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f);

            enemy.Aim(pos);
            // enemy.Aim(enemy.IsTargetInSight(targetTransform.position) ? targetTransform.position : targetPosition);          

            // If a door has shut in the way of the path, remove the rest of the path
            // TODO: This almost certainly will need to be updated to become more robust
            if (Physics2D.OverlapPoint(pos, LayerMask.GetMask("Doors")))
            {
                pathToTarget.RemoveRange(pathToTarget.IndexOf(targetPosition), pathToTarget.Count);
                return;
            }

            rigidBody.position = Vector2.MoveTowards
                (enemy.transform.position, new Vector2(targetPosition.x + 0.5f, targetPosition.y + 0.5f), movementThisFrame);

            if (pathfinding.GetCellPosition(enemy.transform.position) == targetPosition)
                pathToTarget.Remove(targetPosition);
        }
    }
}
