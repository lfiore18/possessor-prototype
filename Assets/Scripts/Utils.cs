using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float RotatationAngleToTarget(Vector2 position, Vector2 target)
    {
        // Find the direction to look in by subtracting the current position of this game object from the target position in world co-ordinates
        Vector2 lookDirection = target - position;
        return Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f; // Returns the angle between positive "x" and "x, y = 1"
    }
}
