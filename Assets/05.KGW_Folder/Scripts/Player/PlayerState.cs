using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
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
    public float MaxTouchTime {  get { return _maxTouchTime; } set { _maxTouchTime = value; }}

    // Player Jump X Direction
    [SerializeField] float _jumpXDir = 0.2f;
    public float JumpXDir { get { return _jumpXDir; } set { _jumpXDir = value; }}

    // Player Jump Y Direction
    [SerializeField] float _jumpYDir = 1f;
    public float JumpYDir { get { return _jumpYDir; } set { _jumpYDir = value; } }
}
