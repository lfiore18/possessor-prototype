using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    public OnAlarmStarted onAlarmStarted;
    public OnAlarmTimerExpired onAlarmTimerExpired;

    [SerializeField] float alarmLastsFor = 20f;
    [SerializeField] float alarmTimer;
    public bool isAlarmed = false;

    bool hasTimerStarted = false;

    // Enemies will call in to update this
    Vector2 targetlastKnownPos;

    // keep track of which enemies can currently see player
    [SerializeField] List<Enemy> enemiesInSightOfTarget = new List<Enemy>();

    // Start is called before the first frame update
    void Start()
    {
        if (onAlarmStarted == null) onAlarmStarted = new OnAlarmStarted();
        if (onAlarmTimerExpired == null) onAlarmTimerExpired = new OnAlarmTimerExpired();

        ResetAlarmTimer();
    }

    private void Update()
    {
/*        if (!IsTargetInSight() && isAlarmed)
            StartAlarmTimer();

        if (!IsTargetInSight() && hasTimerStarted && alarmTimer > 0)
        {
            alarmTimer -= Time.deltaTime;
        }
        else if (isAlarmed)
        {
            StopAlarm();
            onAlarmTimerExpired.Invoke();
        }*/
    }

    public void RaiseAlarm()
    {
        Debug.Log("Alarm raised");
        isAlarmed = true;
        onAlarmStarted.Invoke();
    }

    public void StartAlarmTimer()
    {
        hasTimerStarted = true;
    }

    public void StopAlarm()
    {
        isAlarmed = false;
        hasTimerStarted = false;
        alarmTimer = alarmLastsFor;
    }

    public void ResetAlarmTimer()
    {
        alarmTimer = alarmLastsFor;
    }

    public float AlarmTimeLeft()
    {
        return alarmTimer;
    }

    public bool GetAlarmStatus()
    {
        return isAlarmed;
    }

    public void UpdateLastKnownPosition(Vector2 newPosition)
    {
        targetlastKnownPos = newPosition;
    }

    public Vector2 GetLastKnownPosition()
    {
        return targetlastKnownPos;
    }

    
    bool IsTargetInSight()
    {
        // check to see if there is at least one enemy with a visual on target
        return enemiesInSightOfTarget.Count > 0; 
    }


    public void AddSelfToNetwork(Enemy enemy)
    {
        if (!enemiesInSightOfTarget.Contains(enemy))
            enemiesInSightOfTarget.Add(enemy);
    }

    public void RemoveSelfFromNetwork(Enemy enemy)
    {
        if (enemiesInSightOfTarget.Contains(enemy))
            enemiesInSightOfTarget.Remove(enemy);
    }
}
