using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class PlayerSpottedEvent : UnityEvent { }

public class AlertSystem : MonoBehaviour
{
    public static Transform PlayerTransform;
    public UnityEvent playerSpottedEvent;
    public bool enemiesAlerted = false;

    // Start is called before the first frame update
    void Start()
    {
        if (playerSpottedEvent == null) playerSpottedEvent = new UnityEvent();
        if (PlayerTransform == null) PlayerTransform = GameObject.Find("Player").transform;

        Debug.Log(PlayerTransform);
    }

    public void SetAlert()
    {
        if (!enemiesAlerted)
            enemiesAlerted = true;

        playerSpottedEvent.Invoke();
    }

    public void AddListener(UnityAction action)
    {
        Debug.Log(action.Target + " added to playerSpottedEvent");
        playerSpottedEvent.AddListener(action);
    }

    public bool GetStatus()
    {
        return enemiesAlerted;
    }
}
