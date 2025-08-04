using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController_Map2 : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private LayerMask _groundLayer;

    private LineRenderer _lineRenderer;
    private Animator _animator;
    private Rigidbody2D _rigid;
    private PlayerProperty _player;
    private DistanceJoint2D _joint;
    private SpriteRenderer _playerRenderer;
    private Vector2 _moveDir;
    private GameObject _partner;
    private Vector3 _networkPos;
    private Quaternion _networkRot;
    private float _smooth = 15f;
    
    private bool _isGround;
    private bool _isOnTouch;
    private bool _isOffTouch;
    private bool _isLinked;
    private bool _isInputBlocked;
    private bool _isBounce;

    private float _touchStartTime;
    private float _touchEndTime;
   
    private int _currentAnimatorHash;
    private int _receiveAnimatorHash;
    
    private readonly int Idle_Hash = Animator.StringToHash("ChickenIdle");
    private readonly int Glide_Hash = Animator.StringToHash("ChickenGlide");
    private readonly int Jump_Hash = Animator.StringToHash("ChickenJump");
    
    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _player = GetComponent<PlayerProperty>();
        _joint = GetComponent<DistanceJoint2D>();
        _animator = GetComponentInChildren<Animator>();
        _playerRenderer = GetComponentInChildren<SpriteRenderer>();
        _lineRenderer = GetComponent<LineRenderer>();
        _joint.enabled = false;
        _lineRenderer.enabled = false;
        
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 20;
    }

    private void Start()
    {
        // 자신인 경우
        if (photonView.IsMine)
        {
            Camera.main.GetComponent<CameraController_Map2>().SetTarget(transform);
            GameManager_Map2.Instance.OnReadyGame += () => SetJoint();
            GameManager_Map2.Instance.SetPlayer(transform);
            GameManager_Map2.Instance.OnPanelOpened += SetInputBlocked;
            GameManager_Map2.Instance.OnReachGoal += ChangeCamera;
        }
        // 자신이 아닌 경우 투명도 낮추기
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
            if (!_isInputBlocked)
            {
#if UNITY_EDITOR
                TouchInput_Test();
#endif
                TouchInput();
            }
            if (_isBounce && _partner !=null)
            {
                float dist = Vector2.Distance(transform.position, _partner.transform.position);
                if (dist >= _joint.distance - 0.05f)
                {
                    Vector2 dir = transform.position - _partner.transform.position;
                    _partner.GetComponent<Rigidbody2D>().velocity = dir * _rigid.velocity.magnitude;
                    _partner.GetComponent<Rigidbody2D>().mass = 0.01f;
                }
            }
            PlayerStateUpdate();
            if(_partner != null)
            {
                Vector2 posA = (Vector2)transform.position + Vector2.up * 0.2f;
                Vector2 posB = (Vector2)_partner.transform.position + Vector2.up * 0.2f;
                _lineRenderer.SetPosition(0, posA);
                _lineRenderer.SetPosition(1, posB);
            }
        }
    }

    // Rigidbody2D의 속성을 변경하는 경우 FixedUpdate에서 처리
    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            SetGravity();
            PlayerJump();
            CheckGround();
        }
        if (!photonView.IsMine) 
        {
            transform.position = Vector3.Lerp(transform.position, _networkPos, Time.fixedDeltaTime * _smooth);
            transform.rotation = Quaternion.Lerp(transform.rotation, _networkRot, Time.fixedDeltaTime * _smooth);
        }
    }
    
    // GroundLayer에 2개 이상의 layer가 포함될 수 있어 비트 연산으로 코드 수정
    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("진입5");
        Debug.Log($"[DEBUG] 이 오브젝트의 Owner: {photonView.Owner.NickName}, IsMine: {photonView.IsMine}");
        
        if (photonView.IsMine)
        {
            Debug.Log("진입6");
            if(((1 << collision.gameObject.layer) & _groundLayer) != 0)
            {
                Debug.Log("진입7");
                _isGround = true;
            }
        }
    }*/

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

        if (photonView.IsMine)
        {
            if(collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
            {
                GameManager_Map2.Instance.ReachGoalPoint();
            }
        }
    }
    
    private void PlayerStateUpdate()
    {
        if (_rigid.velocity == Vector2.zero)
        {
            _currentAnimatorHash = Idle_Hash;
            _animator.Play(Idle_Hash);
        }
        else if (_rigid.velocity.y < 0)
        {
            _currentAnimatorHash = Glide_Hash;
            _animator.Play(Glide_Hash);
        }
    }
    
    private void ChangeCamera()
    {
        // 다른 플레이어의 카메라로 관찰
        Camera.main.GetComponent<CameraController_Map2>().OnViewingMode();
    }
    
    // player의 투명도를 다시 1로 리셋하는 메서드
    private void ResetAlpha(GameObject player)
    {
        Color color = player.GetComponentInChildren<SpriteRenderer>().color;
        color.a = 1f;
        player.GetComponentInChildren<SpriteRenderer>().color = color;
    }
    
    // 투명도 조절 메서드
    private void SetAlpha(float value)
    {
        Color color = _playerRenderer.color;
        color.a = value;
        _playerRenderer.color = color;
    }

    private void SetInputBlocked(bool isOpen)
    {
        _isInputBlocked = isOpen;
    }
    
    // 같은 팀의 두 플레이어를 줄로 묶는 메서드
    private void SetJoint()
    {
        if (_isLinked) return;
        
        // 네트워크에 저장된 커스텀 프로퍼티의 내 팀
        string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["Color"] as string;
        
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            // 자신인 경우 스킵
            if(player == gameObject) continue;
            
            // 플레이어의 PhotonView를 가져오기
            PhotonView otherPlayer = player.GetComponent<PhotonView>();
            
            // PhotonView가 없으면 스킵
            if (otherPlayer == null) continue;
            
            // 네트워크에 저장된 커스텀 프로퍼티의 다른 플레이어의 팀
            string team = otherPlayer.Owner.CustomProperties["Color"] as string;
            
            // 해당 팀이 내 팀과 같은 경우
            if (team == myTeam)
            {
                Rigidbody2D target = player.GetComponent<Rigidbody2D>();

                // Distance-Joint 연결, 투명도 리셋
                if (target != null)
                {
                    _joint.connectedBody = target;
                    _joint.autoConfigureDistance = false;
                    _joint.distance = 1f;
                    _joint.enabled = true;
                    _isLinked = true;
                    _partner = player.gameObject;
                    ResetAlpha(player);

                    _lineRenderer.enabled = true;
                    _lineRenderer.positionCount = 2;
                    _lineRenderer.startWidth = 0.02f;
                    _lineRenderer.endWidth = 0.02f;
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
            _rigid.gravityScale = 1f;
            _isBounce = false;
            if (_partner?.GetComponent<Rigidbody2D>().mass < 0.5f)
            {
                _partner.GetComponent<Rigidbody2D>().mass = 1f;
            }
        }
        else
        {
            _rigid.gravityScale = 1f;
        }

        if (!_isBounce && _isGround && _partner?.GetComponent<Rigidbody2D>().mass < 0.5f)
        {
            _partner.GetComponent<Rigidbody2D>().mass = 1f;
        }
    }

    // 빌드 시 인게임에서 사용자의 터치를 입력받는 메서드
    private void TouchInput()
    {
        if (Application.isMobilePlatform)
        {
            if (!_isGround || _rigid.velocity.y > 3) return;
            
            // 화면 터치 여부는 터치하고 있는 손가락 수로 판단
            if (Input.touchCount > 0)
            {
                // 입력된 터치 정보 반환(struct)
                Touch touch = Input.GetTouch(0);
								
                // 터치가 시작되는 시점
                if (touch.phase == TouchPhase.Began)
                {
                    if (touch.position.y < Screen.height / 10) return;
                    // 터치가 된 시각 캐싱
                    _touchStartTime = Time.time;
                    _isOnTouch = true;
                    
                    // 모바일 스크린 기준 중심 x좌표(width)
                    int centerOfScreen = Screen.width / 2;
		                
                    // 더 작으면 = 중심 기준 왼쪽을 터치한 경우
                    if (touch.position.x < centerOfScreen)
                    {
                        _moveDir = _player.MoveLeftDir.normalized;
                        _playerRenderer.flipX = true;
                    }
                    // 더 크면 = 중심 기준 오른쪽을 터치한 경우
                    else
                    {
                        _moveDir = _player.MoveRightDir.normalized;
                        _playerRenderer.flipX = false;
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
        if (!_isGround || _rigid.velocity.y > 3) return;
        // 화면 터치 시 터치 타임 측정
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.y < Screen.height / 10) return;
            
            _touchStartTime = Time.time;
            _isOnTouch = true;
            
            int centerOfScreen = Screen.width / 2;
            if (mousePos.x < centerOfScreen)
            {
                _moveDir = _player.MoveLeftDir.normalized;
                _playerRenderer.flipX = true;
            }
            else
            {
                _moveDir = _player.MoveRightDir.normalized;
                _playerRenderer.flipX = false;
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
            
            _currentAnimatorHash = Jump_Hash;
            _animator.Play(Jump_Hash);
            AudioManager_Map2.Instance.PlaySFX(AudioManager_Map2.Sfxs.SFX_GooseJump);

            // 플래그 초기화
            _isGround = false;
            _isOnTouch = false;
            _isOffTouch = false;
        }
    }
    
    // Bounce Tile을 밟았을 때 호출되는 메서드
    public void Bounce(float power)
    {
        if (_rigid.velocity.y <= 0)
        {
            _rigid.AddForce(Vector2.up * power, ForceMode2D.Impulse);
            _isBounce = true;
            AudioManager_Map2.Instance.PlaySFX(AudioManager_Map2.Sfxs.SFX_Jump);
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 동기화 데이터 보내기
        if (stream.IsWriting)
        {
            stream.SendNext(_currentAnimatorHash);  // 현재 플레이어의 애니메이션 데이터 전송
            stream.SendNext(_playerRenderer.flipX); // 플레이어의 방향 데이터 전송
            stream.SendNext(_playerRenderer.flipY); // 플레이어의 방향 데이터 전송
            
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else    // 동기화 데이터 받기
        {
            _receiveAnimatorHash = (int)stream.ReceiveNext();    // 애니메이션 정보 받기
            _animator.Play(_receiveAnimatorHash);   // 받은 정보로 애니메이션 플레이
            _playerRenderer.flipX = (bool)stream.ReceiveNext();
            _playerRenderer.flipY = (bool)stream.ReceiveNext();
            
            _networkPos = (Vector3) stream.ReceiveNext();
            _networkRot = (Quaternion) stream.ReceiveNext();
        }
    }

    private void CheckGround()
    {
        Vector2 origin = (Vector2)transform.position;
        Vector2 right = (Vector2)transform.position + Vector2.right * 0.1f;
        Vector2 left = (Vector2)transform.position + Vector2.left * 0.1f;
        
        float distance = 0.01f;
        RaycastHit2D hit1 = Physics2D.Raycast(origin, Vector2.down, distance,_groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(right, Vector2.down, distance,_groundLayer);
        RaycastHit2D hit3 = Physics2D.Raycast(left, Vector2.down, distance,_groundLayer);

        if (hit1.collider != null || hit2.collider != null || hit3.collider != null)
        {
            _isGround = true;
            _isBounce = false;
        }
    }
}
