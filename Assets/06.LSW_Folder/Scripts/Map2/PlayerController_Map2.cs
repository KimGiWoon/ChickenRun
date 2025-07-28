using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class PlayerController_Map2 : MonoBehaviourPun
{
    [SerializeField] private LayerMask _groundLayer;
    
    private Rigidbody2D _rigid;
    private PlayerProperty _player;
    private DistanceJoint2D _joint;
    private SpriteRenderer _playerRenderer;
    private Vector2 _moveDir;
    
    private bool _isGround;
    private bool _isOnTouch;
    private bool _isOffTouch;
    private bool _isLinked;

    private float _touchStartTime;
    private float _touchEndTime;
    
    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _player = GetComponent<PlayerProperty>();
        _joint = GetComponent<DistanceJoint2D>();
        _joint.enabled = false;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Camera.main.GetComponent<CameraController_Map2>().SetTarget(transform);
            GameManager_Map2.Instance.OnStartGame += () => SetJoint();
        }
        else
        {
            SetAlpha(0.5f);
        }
    }
    
    // 마우스, 터치 입력은 Update에서 처리
    // 마우스 입력은 전처리기를 통해 Editor 상에서만 사용하고 빌드 시 포함x
    private void Update()
    {
        if (photonView.IsMine)
        {
#if UNITY_EDITOR
            TouchInput_Test();
#endif
            TouchInput();
        }
    
    }

    // Rigidbody2D의 속성을 변경하는 경우 FixedUpdate에서 처리
    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            SetGravity();
            PlayerJump();
        }
    }
    
    // GroundLayer에 2개 이상의 layer가 포함될 수 있어 비트 연산으로 코드 수정
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(((1 << collision.gameObject.layer) & _groundLayer) != 0)
        {
            _isGround = true;
        }
    }

    // 트리거 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 달걀 획득
        if (collision.gameObject.layer == LayerMask.NameToLayer("Egg"))
        {
            // 게임매니저의 알 개수 증가
            GameManager_Map2.Instance.GetEgg();
            Destroy(collision.gameObject);
        }
        
        if(collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            GameManager_Map2.Instance.ReachGoalPoint();
        }
    }

    private void ResetAlpha(GameObject player)
    {
        Color color = player.GetComponent<SpriteRenderer>().color;
        color.a = 1f;
        player.GetComponent<SpriteRenderer>().color = color;
    }
    
    private void SetAlpha(float value)
    {
        Color color = _playerRenderer.color;
        color.a = value;
        _playerRenderer.color = color;
    }
    
    // 같은 팀의 두 플레이어를 줄로 묶는 메서드
    private void SetJoint()
    {
        if (_isLinked) return;
        
        string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["Team"] as string;
        
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if(player == gameObject) continue;
            PhotonView otherPlayer = player.GetComponent<PhotonView>();
            if (otherPlayer == null) continue;
            
            string team = otherPlayer.Owner.CustomProperties["Team"] as string;
            if (team == myTeam)
            {
                Rigidbody2D target = player.GetComponent<Rigidbody2D>();

                if (target != null)
                {
                    _joint.connectedBody = target;
                    _joint.autoConfigureDistance = false;
                    _joint.distance = 1f;
                    _joint.enabled = true;
                    _isLinked = true;
                    ResetAlpha(player);
                    break;
                } 
            } 
        }
    }
    
    // 떨어질 때 중력값 보정
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

    // 빌드 시 인게임에서 사용자의 터치를 입력받는 메서드
    private void TouchInput()
    {
        if (Application.isMobilePlatform)
        {
            if (!_isGround) return;
            // 화면 터치 여부는 터치하고 있는 손가락 수로 판단
            if (Input.touchCount > 0)
            {
                // 입력된 터치 정보 반환(struct)
                Touch touch = Input.GetTouch(0);
								
                // 터치가 시작되는 시점
                if (touch.phase == TouchPhase.Began)
                {
                    // 터치가 된 시각 캐싱
                    _touchStartTime = Time.time;
                    _isOnTouch = true;
                    
                    // 모바일 스크린 기준 중심 x좌표(width)
                    int centerOfScreen = Screen.width / 2;
		                
                    // 더 작으면 = 중심 기준 왼쪽을 터치한 경우
                    if (touch.position.x < centerOfScreen)
                    {
                        _moveDir = _player.MoveLeftDir.normalized;
                    }
                    // 더 크면 = 중심 기준 오른쪽을 터치한 경우
                    else
                    {
                        _moveDir = _player.MoveRightDir.normalized;
                    }
                }
                
                // 터치가 끝나는 시점 + 땅에 있어야하고, 위에서 입력된 터치일 것
                else if (touch.phase == TouchPhase.Ended && _isGround && _isOnTouch)
                {
                    // 터치가 끝난 시각 캐싱
                    _touchEndTime = Time.time;
                    _isOffTouch = true;
                }
            }
        }
    }

    // 에디터에서 사용할 테스트 코드. 사용자의 터치를 마우스로 입력받는 메서드
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
            _touchEndTime = Time.time;
            _isOffTouch = true;
        }
    }
#endif
    
    // 플레이어 점프
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
    
    // Bounce Tile을 밟았을 때 호출되는 메서드
    public void Bounce(float power)
    {
        _rigid.velocity = new Vector2(_rigid.velocity.x, 0);
        _rigid.AddForce(Vector2.up * power, ForceMode2D.Impulse);
    }
}
