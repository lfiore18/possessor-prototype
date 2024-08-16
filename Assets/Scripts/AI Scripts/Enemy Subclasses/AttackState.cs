using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : EnemyState
{
    Transform targetTransform;

    public AttackState(Enemy controller, Transform targetTransform) : base(controller) 
    {
        this.targetTransform = targetTransform;
    }

    public override void Enter()
    {
        Debug.Log("Entering Attack State");
    }

    public override void Execute()
    {
        if (controller.IsTargetInSight() && controller.IsTargetInAttackRange())
        {
            controller.Aim(targetTransform.position);
            controller.combatBehaviour.Attack();            
        }
        else
        {
            controller.ChangeState(new ChaseState(controller, targetTransform));
        }

        Debug.Log("Attacking Target");
    }

    public override void Exit()
    {
        controller.combatBehaviour.StopAttacking();
        Debug.Log("Exiting Attack State");
    }
}
