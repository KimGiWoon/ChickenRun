using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController_Map1 : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    [Header("Player Setting Reference")]
    [SerializeField] PlayerState_Map1 _playerstate;
    [SerializeField] SpriteRenderer _playerRenderer;
    [SerializeField] PlayerEmoticonController_Map1 _playerEmoticonController;
    [SerializeField] Slider _jumpGuageSlider;
    [SerializeField] Animator _playerAni;

    [Header("Correction Setting")]
    [SerializeField] float _correctionValue = 15f;

    public UIManager_Map1 _gameUIManager;
    public GameManager_Map1 _gameManager;
    public Rigidbody2D _playerRigid;
    BoostController _boostController;
    Vector2 _jumpDir;
    Vector3 _currentPosition;
    Quaternion _currentRotation;
    float _touchStartTime;
    float _touchEndtTime;
    float _touchDuration;
    float _guageValue;
    public bool _isGround;
    bool _isTouch;
    public bool _isGoal = false;
    int _currentAnimatorHash;
    int _reciveAnimatorHash;

    // 플레이어 애니메이션
    public readonly int Idle_Hash = Animator.StringToHash("Idle");
    public readonly int Glide_Hash = Animator.StringToHash("Glide");
    public readonly int Jump_Hash = Animator.StringToHash("Jump");

    private void Awake()
    {
        _playerRigid = GetComponent<Rigidbody2D>();
        _boostController = GetComponent<BoostController>();

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
            _gameManager.OnPlayerGoal += ChangeCamera;

            // 플레이어 시작위치 저장
            _gameManager.StartPosSave(transform);
        }
        
        // 자기 자신의 카메라 설정
        if (photonView.IsMine) 
        {
            _gameUIManager.SetPlayerPosition(transform);
            ChangeCamera();

            // 점프게이지 활성화
            _jumpGuageSlider.gameObject.SetActive(true);

            // 부스터 컨트롤러 전달
            _gameUIManager.SetBoostController(_boostController);
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

    private void OnDestroy()
    {
        if(_gameManager != null)
        {
            // 카메라 세팅 이벤트 구독 취소
            _gameManager.OnPlayerGoal -= ChangeCamera;
        }
    }

    // 매니저 초기화
    private void ManagerInit()
    {
        if (_gameUIManager == null)
        {
            _gameUIManager = FindObjectOfType<UIManager_Map1>();
        }

        if(_gameManager == null)
        {
            _gameManager = FindObjectOfType<GameManager_Map1>();
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
            if (_gameUIManager._isOptionOpen || _gameUIManager._isEmoticonPanelOpen || _isGoal) return;

            if (!_isTouch)
            {
                _touchStartTime = Time.time;
                _isTouch = true;
            }

            // 터치 중일 때 점프 파워 게이지 증가
            float elapsed = Time.time - _touchStartTime;
            _guageValue = Mathf.Clamp01(elapsed / _playerstate.MaxTouchTime);
            _jumpGuageSlider.value = _guageValue;
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
            _touchEndtTime = Time.time;
            _currentAnimatorHash = Jump_Hash;
            _playerAni.Play(Jump_Hash);

            // 0 -> 1의 값으로 부드럽게 점프 동작을 보정
            _touchDuration = Mathf.Clamp01((_touchEndtTime - _touchStartTime) / _playerstate.MaxTouchTime);
            float jumpPower = Mathf.Lerp(_playerstate.JumpPower, _playerstate.MaxJumpPower, _touchDuration);

            // 앞으로의 점프 방향
            _jumpDir = new Vector2(_playerstate.JumpXDir, _playerstate.JumpYDir).normalized;
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
        if (_playerRigid.velocity == Vector2.zero) {
            _currentAnimatorHash = Idle_Hash;
            _playerAni.Play(Idle_Hash);
        }
        else if (_playerRigid.velocity.y < 0) {
            _currentAnimatorHash = Glide_Hash;
            _playerAni.Play(Glide_Hash);
        }
    }

    // 플레이어가 골인하면 카메라 전환
    public void ChangeCamera()
    {
        if (_isGoal)
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            _isGround = true;
            _playerRigid.velocity = Vector2.zero;
        }
    }

    // 트리거 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 달걀 획득
        if (collision.gameObject.layer == LayerMask.NameToLayer("Egg")) {
            if (photonView.IsMine) {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_GetEgg);
                // 게임매니저의 알 개수 증가
                _gameManager.GetEgg(1);
                Destroy(collision.gameObject);
            }
        }

        // 아이템 획득
        if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            // 아이템 획득 시 중복 획득 불가
            if (_boostController._hasBoostItem) return;

            if (photonView.IsMine)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_GetEgg);

                _boostController.GetBoostItem();
                //Destroy(collision.gameObject);
            }
        }

        // 물에 입수
        if (collision.gameObject.layer == LayerMask.NameToLayer("Water")) {
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_DropWater);
            Debug.Log("물에 접촉");
            // 게임의 처음 위치로 이동
            _playerRigid.velocity = Vector2.zero;
            gameObject.transform.position = _gameManager._startPos;
        }

        // 체크 포인트 접촉
        if (collision.gameObject.layer == LayerMask.NameToLayer("CheckPoint")) {
            // 자신의 리스폰 위치 변경
            if (photonView.IsMine) 
            {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_CheckPoint);
                _gameManager._startPos = collision.transform.position;
            }
        }

        // 결승점 도착
        if (collision.gameObject.layer == LayerMask.NameToLayer("Goal")) {
            // 골을 했으면 넘어가기
            if (_isGoal) return;

            // 내 플레이어만 처리
            if (photonView.IsMine) 
            {
                string playerNickname = _playerEmoticonController._nickname;
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Goal);

                Debug.Log("결승선 도착");
                _isGoal = true;
                _gameManager.StopStopWatch();

                // 도착을 알림
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
        if (stream.IsWriting) {
            stream.SendNext(transform.position);    // 플레이어의 위치 데이터 전송
            stream.SendNext(transform.rotation);    // 플레이어의 회전 데이터 전송
            stream.SendNext(_currentAnimatorHash);  // 현재 플레이어의 애니메이션 데이터 전송
        }
        else    // 동기화 데이터 받기
        {
            _currentPosition = (Vector3)stream.ReceiveNext();
            _currentRotation = (Quaternion)stream.ReceiveNext();
            _reciveAnimatorHash = (int)stream.ReceiveNext();    // 애니메이션 정보 받기
            _playerAni.Play(_reciveAnimatorHash);   // 받은 정보로 애니메이션 플레이
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
                        Debug.Log("듀드몬스터 스킨입니다.");
                        controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/DudeMonsterAnimation/DudeMonsterAnimatorController");
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
