using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProperty : MonoBehaviour
{
    public float MinJumpPower { get; private set; }
    public float MaxJumpPower { get; private set; }
    public float MaxTouchTime { get; private set; }
    public Vector2 MoveLeftDir { get; private set; }
    public Vector2 MoveRightDir { get; private set; }

    private void Awake()
    {
        MinJumpPower = 1f;
        MaxJumpPower = 7f;
        MaxTouchTime = 1.5f;
        MoveLeftDir = new Vector2(-0.3f, 1f);
        MoveRightDir = new Vector2(0.3f, 1f);
    }
}
