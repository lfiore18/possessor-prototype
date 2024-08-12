using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState
{
    protected AIStateController controller;

    public AIState(AIStateController controller)
    {
        this.controller = controller;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
