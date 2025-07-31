using System.Collections;
using System.Collections.Generic;
using System;
using Photon.Pun;
using UnityEngine;

public class PlayerController_Map4 : MonoBehaviourPun, IPunObservable
{
    [Header("Player Setting Reference")]
    [SerializeField] PlayerState_Map4 _playerstate;
    [SerializeField] SpriteRenderer _playerRenderer;
    [SerializeField] Animator _playerAni;

    [Header("Correction Setting")]
    [SerializeField] float _correctionValue = 20f;

    UIManager_Map4 _gameUIManager;
    Rigidbody2D _playerRigid;
    Vector2 _jumpDir;
    Vector3 _currentPosition;
    Quaternion _currentRotation;
    float _touchStartTime;
    float _touchEndTime;
    bool _isGround;
    bool _isTouch;
    public bool _isDeath = false;
    int _currentAnimatorHash;
    int _reciveAnimatorHash;

    // Idle 애니메이션
    public readonly int Idle_Hash = Animator.StringToHash("ChickenIdle");
    public readonly int Glide_Hash = Animator.StringToHash("ChickenGlide");
    public readonly int Jump_Hash = Animator.StringToHash("ChickenJump");

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
        // 카메라 세팅 이벤트 구독
        GameManager_Map4.Instance.OnPlayerDeath += ChangeCamera;

        // 자기 자신의 카메라 설정
        if (photonView.IsMine)
        {
            GameManager_Map4.Instance._gameUIManager.SetPlayerPosition(transform);
            ChangeCamera();
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
        // 카메라 세팅 이벤트 구독 취소
        GameManager_Map4.Instance.OnPlayerDeath -= ChangeCamera;
    }

    // 화면 터치 입력
    private void TouchInput()
    {
        if (!_isGround) return;

        // 화면 터치 시 터치 타임 측정
        if (Input.GetMouseButtonDown(0))
        {
            // 옵션창 오픈 시 움직임 금지
            if (_gameUIManager._isOptionOpen) return;

            // 결승점 통과 시 움직임 금지
            if (GameManager_Map4.Instance._isGoal) return;

            _touchStartTime = Time.time;
            _isTouch = true;

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
    }

    // 플레이어 점프
    private void PlayerJump()
    {
        if (!_isGround) return;
        // 화면 터치 끝
        if (Input.GetMouseButtonUp(0) && _isGround && _isTouch)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Jump);
            _touchEndTime = Time.time - _touchStartTime;
            _currentAnimatorHash = Jump_Hash;
            _playerAni.Play(Jump_Hash);

            // 0 -> 1의 값으로 부드럽게 점프 동작을 보정
            float maxTime = Mathf.Clamp01(_touchEndTime / _playerstate.MaxTouchTime);
            float jumpPower = Mathf.Lerp(_playerstate.JumpPower, _playerstate.MaxJumpPower, maxTime);

            // 앞으로의 점프 방향
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

    // 카메라 전환
    public void ChangeCamera()
    {
        if (_isDeath)
        {
            // 다른 플레이어의 카메라로 관찰
            FindObjectOfType<CameraController>().OnViewingMode();
        }
        else
        {
            Camera.main.GetComponent<CameraController>().SetTarget(transform);
            GameManager_Map4.Instance._gameUIManager.SetPlayerPosition(transform);
            _gameUIManager = GameManager_Map4.Instance._gameUIManager;
        }
    }

    // 물리적 충돌
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGround = true;
        }

        // 드릴에 충돌
        if (collision.gameObject.layer == LayerMask.NameToLayer("Drill"))
        {
            _isDeath = true;
            gameObject.SetActive(false);

            GameManager_Map4.Instance.PlayerDeath();
            //// 방장에게 알리기
            //if (PhotonNetwork.IsMasterClient)
            //{
            //    GameManager_Map4.Instance.PlayerDeath();
            //}
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
                GameManager_Map4.Instance.GetEgg(1);
                Destroy(collision.gameObject);
            }
        }

        // 결승점 도착
        if (collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            // 골을 했으면 넘어가기
            if (GameManager_Map4.Instance._isGoal) return;

            // 내 플레이어만 처리
            if (photonView.IsMine)
            {
                string playerNickname = PhotonNetwork.LocalPlayer.NickName;
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Goal);

                // 도착을 알림
                photonView.RPC(nameof(ArrivePlayer), RpcTarget.AllViaServer, playerNickname);
                Debug.Log("결승선 도착");

                GameManager_Map4.Instance.StopStopWatch();
            }
        }
    }

    // 도착한 플레이어
    [PunRPC]
    public void ArrivePlayer(string playerNickname)
    {
        Debug.Log($"{playerNickname}께서 결승점에 도착했습니다.");
        GameManager_Map4.Instance.PlayerReachedGoal(playerNickname);
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
}
