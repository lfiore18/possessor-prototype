using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TimerUI : MonoBehaviour
{
    [SerializeField] Text alarmTimerText;

    public void SetAlarmTimerText(float timeInSecs)
    {
        alarmTimerText.text = timeInSecs.ToString();
    }
}
