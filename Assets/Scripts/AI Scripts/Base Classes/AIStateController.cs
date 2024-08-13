using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateController : MonoBehaviour
{
    public AIState currentState;

    // Start is called before the first frame update
    public void Start()
    {
        currentState.Enter();
    }

    // Update is called once per frame
    public void Update()
    {
        currentState.Execute();
    }

    public void ChangeState(AIState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public Pathfinding GetPathfinder()
    {
        return FindObjectOfType<Pathfinding>();
    }
}
