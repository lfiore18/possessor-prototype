using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState
{
    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
