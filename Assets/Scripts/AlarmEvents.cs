using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// When an enemy spots a player, the alarm will be raised 
public class OnAlarmStarted : UnityEvent { }

public class OnAlarmTimerStarted : UnityEvent { }
public class OnAlarmTimerExpired : UnityEvent { }


public class OnTargetSpotted : UnityEvent { }

// When enemies lose sight of the player 
public class OnTargetLost : UnityEvent { }