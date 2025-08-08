using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Map4 : MonoBehaviour
{
    [Header("Player State")]

    // Player Jump Power
    [SerializeField] float _jumpPower = 3f;
    public float JumpPower { get { return _jumpPower; } set { _jumpPower = value; } }

    // Player Max jump Power
    [SerializeField] float _maxJumpPower = 10f;
    public float MaxJumpPower { get { return _maxJumpPower; } set { _maxJumpPower = value; } }

    // Touch Max Jump Time
    [SerializeField] float _maxTouchTime = 2f;
    public float MaxTouchTime { get { return _maxTouchTime; } set { _maxTouchTime = value; } }

    // Player Jump Left Direction
    [SerializeField] Vector2 _jumpLeftDir = new Vector2(-0.2f, 1f);
    public Vector2 JumpLeftDir { get { return _jumpLeftDir; } }

    // Player Jump Right Direction
    [SerializeField] Vector2 _jumpRightDir = new Vector2(0.2f, 1f);
    public Vector2 JumpRightDir { get { return _jumpRightDir; } }
}
