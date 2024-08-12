using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Text alarmTimerText;
    [SerializeField] Text alarmStatusText;

    // For adding event listeners to the alert system
    [SerializeField] AlertSystem alertSystem;

    private void Start()
    {
        alertSystem.onAlarmStarted.AddListener(AlarmStartedHandler);
        alertSystem.onAlarmTimerExpired.AddListener(AlarmExpiredHandler);
    }

    private void Update()
    {
        if (alertSystem.isAlarmed)
            SetAlarmTimerText(alertSystem.AlarmTimeLeft());
    }

    public void AlarmStartedHandler()
    {
        Debug.Log("From: " + gameObject.name + "Message: Alarm Started");
        SetAlarmStatusUI(true);
    }

    public void AlarmExpiredHandler()
    {
        Debug.Log("From: " + gameObject.name + "Message: Alarm Ended");
        SetAlarmStatusUI(false);
    }

    public void SetAlarmTimerText(float timeInSecs)
    {
        alarmTimerText.text = timeInSecs.ToString();
    }

    public void SetAlarmStatusUI(bool isAlarmed)
    {
        alarmStatusText.text = isAlarmed ? "Status: Alarmed" : "Status: Normal";
        alarmStatusText.color = isAlarmed ? Color.red : Color.green;
    }
}
