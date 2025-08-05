using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_Map4 : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [Header("Player Setting Reference")]
    [SerializeField] PlayerState_Map4 _playerstate;
    [SerializeField] SpriteRenderer _playerRenderer;
    [SerializeField] Animator _playerAni;
    [SerializeField] PlayerEmoticonController_Map4 _playerEmoticonController;
    [SerializeField] Slider _jumpGuageSlider;

    [Header("Correction Setting")]
    [SerializeField] float _correctionValue = 20f;

    UIManager_Map4 _gameUIManager;
    GameManager_Map4 _gameManager;
    Rigidbody2D _playerRigid;
    Vector2 _jumpDir;
    Vector3 _currentPosition;
    Quaternion _currentRotation;
    float _touchStartTime;
    float _touchEndTime;
    float _guageValue;
    float _maxTime;
    bool _isGround;
    bool _isTouch;
    public bool _isGoal = false;
    public bool _isDeath = false;
    int _currentAnimatorHash;
    int _reciveAnimatorHash;

    // 플레이어 애니메이션
    public readonly int Idle_Hash = Animator.StringToHash("Idle");
    public readonly int Glide_Hash = Animator.StringToHash("Glide");
    public readonly int Jump_Hash = Animator.StringToHash("Jump");

    private void Awake()
    {
        _playerRigid = GetComponent<Rigidbody2D>();

        // 조종 가능 유/무에 따른 레이어 설정 (충돌 관련 세팅)
        if (photonView.IsMine)
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
            // 반 투명화
            TranslucentSetting();
        }
    }

    private void Start()
    {
        ManagerInit();

        if (_gameManager != null)
        {
            // 카메라 세팅 이벤트 구독
            _gameManager.OnPlayerDeath += ChangeCamera;
            _gameManager.OnPlayerGoal += ChangeCamera;
        }
        
        // 자기 자신의 카메라 설정
        if (photonView.IsMine)
        {
            _gameUIManager.SetPlayerPosition(transform);
            ChangeCamera();

            // 점프게이지 활성화
            _jumpGuageSlider.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // 자기 자신만 동작
        if (photonView.IsMine)
        {
            PlayerStateUpdate();
            TouchInput();
            PlayerJump();
        }
    }

    private void FixedUpdate()
    {
        // 자신을 제외한 플레이어의 이동 위치 보간
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, _currentPosition, Time.fixedDeltaTime * _correctionValue);
            transform.rotation = Quaternion.Lerp(transform.rotation, _currentRotation, Time.fixedDeltaTime * _correctionValue);
        }
    }

    // 매니저 초기화
    private void ManagerInit()
    {
        if (_gameUIManager == null)
        {
            _gameUIManager = FindObjectOfType<UIManager_Map4>();
        }

        if (_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager_Map4>();
        }
    }

    private void OnDestroy()
    {
        if (_gameUIManager != null)
        {
            // 카메라 세팅 이벤트 구독 취소
            _gameManager.OnPlayerDeath -= ChangeCamera;
            _gameManager.OnPlayerGoal -= ChangeCamera;
        }
    }

    // 화면 터치 입력
    private void TouchInput()
    {
        if (!_isGround) return;

        // 화면 터치 시 터치 타임 측정
        if (Input.GetMouseButton(0))
        {
            // 해당 상황일 때는 움직임 금지
            if (_gameUIManager._isOptionOpen || _isGoal || _isDeath) return;

            if (!_isTouch)
            {
                _touchStartTime = Time.time;
                _isTouch = true;
            }

            // 터치 중일 때 점프 파워 게이지 증가
            float elapsed = Time.time - _touchStartTime;
            _guageValue = Mathf.Clamp01(elapsed / _playerstate.MaxTouchTime);
            _jumpGuageSlider.value = _guageValue;

            Vector3 mousePos = Input.mousePosition;
            // 모바일 스크린 기준 중심 x좌표(width)
            int centerOfScreen = Screen.width / 2;

            // 스크린 기준 왼쪽 터치 시 왼쪽 이동
            if (mousePos.x < centerOfScreen)
            {
                _jumpDir = _playerstate.JumpLeftDir;
                _playerRenderer.flipX = true;
            }
            else    // 오른쪽 터치 시 오른쪽 이동
            {
                _jumpDir = _playerstate.JumpRightDir;
                _playerRenderer.flipX = false;
            }
        }
        else
        {
            _jumpGuageSlider.value = 0f;
        }
    }

    // 플레이어 점프
    private void PlayerJump()
    {
        if (!_isGround) return;

        // 화면 터치 끝나면 점프
        if (Input.GetMouseButtonUp(0) && _isGround && _isTouch)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Jump);
            _touchEndTime = Time.time - _touchStartTime;
            _currentAnimatorHash = Jump_Hash;
            _playerAni.Play(Jump_Hash);

            // 0 -> 1의 값으로 부드럽게 점프 동작을 보정
            float maxTime = Mathf.Clamp01(_touchEndTime / _playerstate.MaxTouchTime);
            float jumpPower = Mathf.Lerp(_playerstate.JumpPower, _playerstate.MaxJumpPower, maxTime);

            _playerRigid.velocity = Vector2.zero;
            _playerRigid.AddForce(_jumpDir * jumpPower, ForceMode2D.Impulse);

            _isGround = false;
            _isTouch = false;
        }
    }

    // 자기 자신을 제외한 플레이어 반 투명화 세팅
    private void TranslucentSetting()
    {
        Color color = _playerRenderer.color;
        color.a = 0.5f;
        _playerRenderer.color = color;
    }

    // 플레이어의 움직임에 대한 애니메이션
    private void PlayerStateUpdate()
    {
        if (_playerRigid.velocity == Vector2.zero)
        {
            _currentAnimatorHash = Idle_Hash;
            _playerAni.Play(Idle_Hash);
        }
        else if (_playerRigid.velocity.y < 0)
        {
            _currentAnimatorHash = Glide_Hash;
            _playerAni.Play(Glide_Hash);
        }
    }

    // 플레이어가 죽으면 카메라 전환
    public void ChangeCamera()
    {
        if (_isDeath || _isGoal)
        {
            // 다른 플레이어의 카메라로 관찰
            _gameManager._cameraController.OnViewingMode();
        }
        else
        {
            _gameManager._cameraController.SetTarget(transform);
            _gameUIManager.SetPlayerPosition(transform);
        }
    }

    // 물리적 충돌
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 벽이면 통과 못하게 속도 제로
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            _playerRigid.velocity = Vector2.zero;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGround = true;
        }

        // 드릴에 충돌, 플레이어가 사라지기전에 충돌 발생으로 조건 처리
        if (collision.gameObject.layer == LayerMask.NameToLayer("Drill") && !_isDeath)
        {
            _isDeath = true;

            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Death);
            string playerNickname = _playerEmoticonController._nickname;

            gameObject.SetActive(false);
            _gameManager.StopStopWatch();
            _gameManager.PlayerDeath(playerNickname);
        }
    }

    // 트리거 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 달걀 획득
        if (collision.gameObject.layer == LayerMask.NameToLayer("Egg"))
        {
            if (photonView.IsMine)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_GetEgg);
                // 게임매니저의 알 개수 증가
                _gameManager.GetEgg(1);
                Destroy(collision.gameObject);
            }
        }

        // 결승점 도착
        if (collision.gameObject.layer == LayerMask.NameToLayer("Goal") && !_isGoal)
        {
            // 내 플레이어만 처리
            if (photonView.IsMine)
            {
                string playerNickname = _playerEmoticonController._nickname;
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Goal);

                _isGoal = true;
                _gameManager.StopStopWatch();

                photonView.RPC(nameof(ArrivePlayer), RpcTarget.AllViaServer, playerNickname);
            }
        }
    }

    // 도착한 플레이어
    [PunRPC]
    public void ArrivePlayer(string playerNickname)
    {
        Debug.Log($"{playerNickname}께서 결승점에 도착했습니다.");
        _gameManager.PlayerReachedGoal(playerNickname);
    }

    // 플레이어 포톤 뷰 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 동기화 데이터 보내기
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);    // 플레이어의 위치 데이터 전송
            stream.SendNext(transform.rotation);    // 플레이어의 회전 데이터 전송
            stream.SendNext(_currentAnimatorHash);  // 현재 플레이어의 애니메이션 데이터 전송
            stream.SendNext(_playerRenderer.flipX); // 플레이어의 방향 데이터 전송
            stream.SendNext(_playerRenderer.flipY); // 플레이어의 방향 데이터 전송
        }
        else    // 동기화 데이터 받기
        {
            _currentPosition = (Vector3)stream.ReceiveNext();
            _currentRotation = (Quaternion)stream.ReceiveNext();
            _reciveAnimatorHash = (int)stream.ReceiveNext();    // 애니메이션 정보 받기
            _playerAni.Play(_reciveAnimatorHash);   // 받은 정보로 애니메이션 플레이
            _playerRenderer.flipX = (bool)stream.ReceiveNext();
            _playerRenderer.flipY = (bool)stream.ReceiveNext();
        }
    }

    // 포톤 네트워크로 플레이어 생성 시 호출
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
                        Debug.Log("올빼미 스킨입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/OwletMonsterAnimation/OwletMonsterAnimatorController");
                        break;
                    case SkinType.Pig:
                        Debug.Log("돼지 스킨입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/PigAnimation/PigAnimatorController");
                        break;
                    case SkinType.PinkMonster:
                        Debug.Log("핑크몬스터 스킨입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/PinkMonsterAnimation/PinkMonsterAnimatorController");
                        break;
                    default:
                        break;
                }

                // 컨트롤러가 Null이 아니라면 스킨에 맞는 컨트롤러 체인지
                if (controller != null)
                {
                    _playerAni.runtimeAnimatorController = controller;
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
