using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController_Map2 : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
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
    private float _smooth = 50f;
    
    private bool _isGround;
    private bool _isOnTouch;
    private bool _isOffTouch;
    private bool _isLinked;
    private bool _isInputBlocked;
    private bool _isBounce;
    private bool _isGoalIn;
    public bool IsGoalIn => _isGoalIn;

    private float _touchStartTime;
    private float _touchEndTime;
   
    private int _currentAnimatorHash;
    private int _receiveAnimatorHash;
    
    private readonly int Idle_Hash = Animator.StringToHash("Idle");
    private readonly int Glide_Hash = Animator.StringToHash("Glide");
    private readonly int Jump_Hash = Animator.StringToHash("Jump");
    
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
        
        PhotonNetwork.SendRate = 15;
        PhotonNetwork.SerializationRate = 10;
    }

    private void Start()
    {
        // 자신인 경우
        if (photonView.IsMine)
        {
            Camera.main.GetComponent<CameraController_Map2>().SetTarget(this);
            GameManager_Map2.Instance.OnReadyGame += () =>
            {
                SetJoint();
            };
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
            if (!_isInputBlocked && GameManager_Map2.Instance.IsStart)
            {
#if UNITY_EDITOR
                TouchInput_Test();
#endif
                TouchInput();
            }
            if (_isBounce && _partner !=null)
            {
                Vector2 dir = Vector2.up + Vector2.right * (transform.position.x - _partner.transform.position.x);
                _partner.GetComponent<Rigidbody2D>().velocity = dir * _rigid.velocity.magnitude;
                _partner.GetComponent<Rigidbody2D>().mass = 0.01f;
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
            SetMass();
            PlayerJump();
            CheckGround();
        }
        if (!photonView.IsMine) 
        {
            float distance = Vector2.Distance(_rigid.position, _networkPos);
            if (distance > 0.8f)
            {
                _rigid.MovePosition(_networkPos); // 순간 보정
            }
            else
            {
                Vector2 newPos = Vector2.Lerp(_rigid.position, _networkPos, Time.fixedDeltaTime * _smooth);
                _rigid.MovePosition(newPos);
            }
            _rigid.MovePosition(_networkPos);
            //transform.rotation = Quaternion.Lerp(transform.rotation, _networkRot, Time.fixedDeltaTime * _smooth);
        }
    }
    
    // GroundLayer에 2개 이상의 layer가 포함될 수 있어 비트 연산으로 코드 수정
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (photonView.IsMine)
        {
            if(((1 << collision.gameObject.layer) & _groundLayer) != 0)
            {
                //_partner.GetComponent<Rigidbody2D>().mass = 1f;
            }
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

        if (collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            if(photonView.IsMine)
            {
                _isGoalIn = true;
                string team = PhotonNetwork.LocalPlayer.CustomProperties["Color"] as string;
                GameManager_Map2.Instance.ReachGoalPoint(team);
            }
            GameManager_Map2.Instance.GoalInPlayer(this);
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
        Debug.Log(_isGoalIn);
        // 다른 플레이어의 카메라로 관찰
        if (_isGoalIn)
        {
            Camera.main.GetComponent<CameraController_Map2>().OnViewingMode();
        }
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
    private void SetMass()
    {
        if (_rigid.velocity.y <= 0)
        {
            _isBounce = false;
            if (_partner != null && _partner?.GetComponent<Rigidbody2D>().mass < 0.5f)
            {
                _partner.GetComponent<Rigidbody2D>().mass = 1f;
            }
        }
    }

    // 빌드 시 인게임에서 사용자의 터치를 입력받는 메서드
    private void TouchInput()
    {
        if (Application.isMobilePlatform)
        {
            if (!_isGround || _rigid.velocity.y > 3 || _isGoalIn) return;
            
            
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
        if (!_isGround || _rigid.velocity.y > 3 || _isGoalIn) return;
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
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Jump);
            
            photonView.RPC(nameof(RPCJump), RpcTarget.Others, jumpPower, _moveDir, PhotonNetwork.Time);

            // 플래그 초기화
            _isGround = false;
            _isOnTouch = false;
            _isOffTouch = false;
        }
    }

    [PunRPC]
    public void RPCJump(float jumpforce, Vector2 moveDir, double sentTime)
    {
        float lag = (float)(PhotonNetwork.Time - sentTime);

        Vector2 vel = moveDir * jumpforce;
        Vector2 pos = vel * lag + 0.5f * Physics2D.gravity * lag * lag;
        
        if (Vector2.Distance(_rigid.position, _rigid.position + pos) > 0.5f)
        {
            _rigid.position += pos;
        }
        else
        {
            Vector2 targetPos = Vector2.Lerp(_rigid.position, _rigid.position + pos, Time.fixedDeltaTime * 200);
            _rigid.MovePosition(targetPos);
            _rigid.velocity = vel + Physics2D.gravity * lag;
        }

        _networkPos = _rigid.position + pos;
        
        _currentAnimatorHash = Jump_Hash;
        _animator.Play(Jump_Hash);
    }
    
    // Bounce Tile을 밟았을 때 호출되는 메서드
    public void Bounce(float power)
    {
        if (_rigid.velocity.y <= 0.5)
        {
            _rigid.AddForce(Vector2.up * power, ForceMode2D.Impulse);
            _isBounce = true;
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Bounce);
            
            photonView.RPC(nameof(RPCJump), RpcTarget.Others, power, Vector2.up, PhotonNetwork.Time);
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
            stream.SendNext(_rigid.velocity); 
        }
        else    // 동기화 데이터 받기
        {
            _receiveAnimatorHash = (int)stream.ReceiveNext(); 
            _playerRenderer.flipX = (bool)stream.ReceiveNext();
            _playerRenderer.flipY = (bool)stream.ReceiveNext();
            
            _networkPos = (Vector3) stream.ReceiveNext();
            
            Vector2 receivedVelocity = (Vector2)stream.ReceiveNext();
            float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
            _networkPos += (Vector3)((receivedVelocity * lag) +  Physics2D.gravity * (0.5f *  lag * lag));
            if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash != _receiveAnimatorHash)
            {
                _animator.Play(_receiveAnimatorHash, 0, lag); // 받은 정보로 애니메이션 플레이
            }   
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
    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // 오브젝트 배열을 저장
        var data = info.photonView.InstantiationData;

        // 데이터가 Null이 아니고, 데이터가 최소 하나 이상 있는지 확인
        if (data != null && data.Length > 0)
        {
            // 첫번째 데이터 스킨
            string skinName = data[0].ToString();

            Debug.Log($"적용 스킨 : {skinName}");
            ApplySkin(skinName);
        }
    }

    // 스킨 적용
    private void ApplySkin(string skinName)
    {
        // 스킨의 스프라이트 불러오기
        Sprite skinSprite = Resources.Load<Sprite>($"Sprites/Skins/{skinName}");
        if (skinSprite != null)
        {
            Debug.Log($"적용 완료 : {skinSprite}");

            // 스킨 적용
            _playerRenderer.sprite = skinSprite;

            // Common의 enum 스킨 타입 확인
            if (Enum.TryParse<SkinType>(skinName, out SkinType skinType))
            {
                Debug.Log($"스킨 컨트롤러 타입은 {skinType}");

                // 애니메이션 컨트롤러 변수 선언 및 Null로 초기화
                RuntimeAnimatorController controller = null;

                // 스킨의 애니메이션 컨트롤러 불러오기
                switch (skinType)
                {
                    case SkinType.Default:
                        Debug.Log("닭 스킨입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/ChickenAnimation/ChickenAnimation");
                        break;
                    case SkinType.OwletMonster:
                        Debug.Log("올빼미 스킨 입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/OwletMonsterAnimation/OwletMonsterAnimatorController");
                        break;
                    case SkinType.Pig:
                        Debug.Log("돼지 스킨 입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/PigAnimation/PigAnimatorController");
                        break;
                    case SkinType.PinkMonster:
                        Debug.Log("핑크몬스터 스킨 입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/PinkMonsterAnimation/PinkMonsterAnimatorController");
                        break;
                    case SkinType.DudeMonster:
                        Debug.Log("파란몬스터 스킨입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/DudeMonsterAnimation/DudeMonsterAnimatorController");
                        break;
                    default:
                        break;
                }

                // 컨트롤러가 Null이 아니라면 스킨에 맞는 컨트롤러 체인지
                if (controller != null)
                {
                    _animator.runtimeAnimatorController = controller;
                }
            }

            Debug.Log("적용이 되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[PlayerController] 스킨 적용 실패: {skinName}");
        }
    }
}
