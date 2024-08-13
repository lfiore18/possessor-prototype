using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyState : AIState
{
    protected Enemy controller;

    public EnemyState(Enemy controller)
    {
        this.controller = controller;
    }
}
