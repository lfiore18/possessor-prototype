using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertSystem : MonoBehaviour
{
    public static Transform PlayerTransform;

    public OnAlarmStarted onAlarmStarted;
    public OnAlarmExpired onAlarmExpired;

    [SerializeField] float alarmLastsFor = 20f;
    [SerializeField] float alarmTimer;
    public bool isAlarmed = false;
    bool hasTimerStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        if (onAlarmStarted == null) onAlarmStarted = new OnAlarmStarted();
        if (onAlarmExpired == null) onAlarmExpired = new OnAlarmExpired();
        if (PlayerTransform == null) PlayerTransform = GameObject.Find("Player").transform;

        ResetAlarmTimer();
    }

    private void Update()
    {
        if (hasTimerStarted && alarmTimer > 0)
            alarmTimer -= Time.deltaTime;
        else if (isAlarmed)
        {
            isAlarmed = false;
            ResetAlarmTimer();
            StopAlarm();
            onAlarmExpired.Invoke();
        }
    }

    public void RaiseAlarm()
    {
        if (!isAlarmed)
            isAlarmed = true;
        if (!hasTimerStarted)
            StartAlarmTimer();

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
}
