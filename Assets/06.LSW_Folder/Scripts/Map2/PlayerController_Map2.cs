using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController_Map2 : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;
    
    private Rigidbody2D _rigid;
    private PlayerProperty _player;
    private Vector2 _moveDir;
    
    private bool _isGround;
    private bool _jumpPressed;
    private bool _isOnTouch;
    private bool _isOffTouch;

    private float _touchStartTime;
    private float _touchEndTime;
    
    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _player = GetComponent<PlayerProperty>();
    }

    private void Update()
    {
    #if UNITY_EDITOR
        TouchInput_Test();
    #endif
        TouchInput();
        SetGravity();
    }

    private void FixedUpdate()
    {
        PlayerJump();
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(((1 << collision.gameObject.layer) & _groundLayer) != 0)
        {
            _isGround = true;
            _rigid.velocity = Vector2.zero;
        }
    }

    // 트리거 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 달걀 획득
        if (collision.gameObject.layer == LayerMask.NameToLayer("Egg"))
        {
            // 게임매니저의 알 개수 증가
            GameManager.Instance.GetEgg(1);
            Destroy(collision.gameObject);
        }
        
        if(collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            // 플레이 시간 정지
            GameManager.Instance.StopPlayTime();
        }
        
    }

    private void SetGravity()
    {
        if (_rigid.velocity.y < 0 && !_isGround)
        {
            _rigid.gravityScale = 1.5f;
        }
        else
        {
            _rigid.gravityScale = 1f;
        }
    }

    private void TouchInput()
    {
        if (Application.isMobilePlatform)
        {
            if (!_isGround) return;
            // 화면 터치 시 터치 타임 측정
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _touchStartTime = Time.time;
                    _isOnTouch = true;
                    
                    int centerOfScreen = Screen.width / 2;
                
                    if (touch.position.x < centerOfScreen)
                    {
                        _moveDir = _player.MoveLeftDir.normalized;
                    }
                    else
                    {
                        _moveDir = _player.MoveRightDir.normalized;
                    }
                }
                
                else if (touch.phase == TouchPhase.Ended && _isGround && _isOnTouch)
                {
                    _isOffTouch = true;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void TouchInput_Test()
    {
        if (!_isGround) return;
        // 화면 터치 시 터치 타임 측정
        if (Input.GetMouseButtonDown(0))
        {
            _touchStartTime = Time.time;
            _isOnTouch = true;

            Vector3 mousePos = Input.mousePosition;
            int centerOfScreen = Screen.width / 2;
            if (mousePos.x < centerOfScreen)
            {
                _moveDir = _player.MoveLeftDir.normalized;
            }
            else
            {
                _moveDir = _player.MoveRightDir.normalized;
            }
        }
        
        if (Input.GetMouseButtonUp(0) && _isGround && _isOnTouch)
        {
            _isOffTouch = true;
        }
    }
#endif
    
    // 플레이어 점프
    private void PlayerJump()
    {
        if (!_isGround) return;
        // 화면 터치 끝
        if (_isOffTouch)
        {
            _touchEndTime = Time.time - _touchStartTime;

            // 0 -> 1의 값으로 부드럽게 점프 동작을 보정
            float touchTime = Mathf.Clamp01(_touchEndTime / _player.MaxTouchTime);
            float jumpPower = Mathf.Lerp(_player.MinJumpPower, _player.MaxJumpPower, touchTime);
            
            _rigid.velocity = Vector2.zero;
            _rigid.AddForce(_moveDir * jumpPower, ForceMode2D.Impulse);

            _isGround = false;
            _isOnTouch = false;
            _isOffTouch = false;
        }
    }
}
