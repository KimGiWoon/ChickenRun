using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundLayer;
    
    private Rigidbody2D _rigid;
    private PlayerProperty _player;
    private Vector2 _moveDir;
    
    private bool _isGround;
    private bool _isOnTouch;
    private bool _isOffTouch;

    private float _touchStartTime;
    private float _touchEndTime;
    
    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _player = GetComponent<PlayerProperty>();
    }

    private void Start()
    {
        Camera.main.GetComponent<CameraController_Map2>().SetTarget(transform);
    }
    private void Update()
    {
        Debug.Log("진입");
        TouchInput_Test();
    }

    // Rigidbody2D의 속성을 변경하는 경우 FixedUpdate에서 처리
    private void FixedUpdate()
    {
        Debug.Log("진입1");
        SetGravity();
        PlayerJump();
    }
    
    // GroundLayer에 2개 이상의 layer가 포함될 수 있어 비트 연산으로 코드 수정
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(((1 << collision.gameObject.layer) & _groundLayer) != 0)
        {
            _isGround = true;
            //_rigid.velocity = Vector2.zero;
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
            _touchEndTime = Time.time;
            _isOffTouch = true;
        }
    }
    
    private void PlayerJump()
    {
        if (!_isGround) return;
        // 터치가 끝나는 시점에 _isOffTouch가 true가 되어 호출
        if (_isOffTouch)
        {
            // 터치 지속시간 = 터치가 끝난 시각 - 터치가 된 시각
            float touchDuration = _touchEndTime - _touchStartTime;

            // 터치 지속시간 기준으로 점프력 보간
            float touchTime = Mathf.Clamp01(touchDuration / _player.MaxTouchTime);
            float jumpPower = Mathf.Lerp(_player.MinJumpPower, _player.MaxJumpPower, touchTime);
            
            _rigid.velocity = Vector2.zero;
            _rigid.AddForce(_moveDir * jumpPower, ForceMode2D.Impulse);

            // 플래그 초기화
            _isGround = false;
            _isOnTouch = false;
            _isOffTouch = false;
        }
    }

    public void Bounce(float power)
    {
        _rigid.velocity = new Vector2(_rigid.velocity.x, 0);
        _rigid.AddForce(Vector2.up * power, ForceMode2D.Impulse);
    }
}
