using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionData {

    public bool Above;
    public bool Below;
    public bool Left;
    public bool Right;

    public bool IsClimbingSlope;
    public bool IsDescendingSlope;
    public bool IsSlidingDownSlope;
    public bool IsFallingThroughPlatform;

    public float SlopeAngle;
    public float PreviousSlopeAngle;
    public Vector2 SlopeNormal;
    public Vector2 MoveAmountOld;
    
    public void Reset() {
        Above = Below = Left = Right = false;
        IsClimbingSlope = IsDescendingSlope = IsSlidingDownSlope = false;
        SlopeNormal = Vector2.zero;
        PreviousSlopeAngle = SlopeAngle;
        SlopeAngle = 0;
    }   
}