using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Player Setting Reference")]
    [SerializeField] PlayerState _playerstate;
    [SerializeField] SpriteRenderer _playerRenderer;
    [SerializeField] Animator _playerAni;

    [Header("Correction Setting")]
    [SerializeField] float _correctionValue = 15f;

    Rigidbody2D _playerRigid;
    Vector2 _jumpDir;
    Vector3 _currentPosition;
    Quaternion _currentRotation;
    float _touchStartTime;
    float _touchEndTime;
    bool _isGround;
    bool _isTouch;

    // Idle 애니메이션
    public readonly int Idle_Hash = Animator.StringToHash("ChickenIdle");
    public readonly int Walk_Hash = Animator.StringToHash("ChickenWalk");
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
        // 플레이어 시작위치 저장
        GameManager.Instance.StartPosSave(transform);
        // 자기 자신의 카메라 설정
        if (photonView.IsMine)
        {
            Camera.main.GetComponent<CameraController>().SetTarget(transform);
        }
    }

    private void Update()
    {
        // 자기 자신만 동작
        if (photonView.IsMine)
        {
            if(_playerRigid.velocity == Vector2.zero)
            {
                _playerAni.Play(Idle_Hash);
            }
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

    // 화면 터치 입력
    private void TouchInput()
    {
        if (!_isGround) return;

        // 화면 터치 시 터치 타임 측정
        if (Input.GetMouseButtonDown(0))
        {
            _touchStartTime = Time.time;
            _isTouch = true;
        }
    }

    // 플레이어 점프
    private void PlayerJump()
    {
        if (!_isGround) return;
        // 화면 터치 끝
        if (Input.GetMouseButtonUp(0) && _isGround && _isTouch)
        {
            _touchEndTime = Time.time - _touchStartTime;
            _playerAni.Play(Jump_Hash);        

            // 0 -> 1의 값으로 부드럽게 점프 동작을 보정
            float maxTime = Mathf.Clamp01(_touchEndTime / _playerstate.MaxTouchTime);
            float jumpPower = Mathf.Lerp(_playerstate.JumpPower, _playerstate.MaxJumpPower, maxTime);

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

    // 물리적 충돌
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGround = true;
            _playerRigid.velocity = Vector2.zero;
        }
    }

    // 트리거 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 달걀 획득
        if(collision.gameObject.layer == LayerMask.NameToLayer("Egg"))
        {
            // 게임매니저의 알 개수 증가
            GameManager.Instance.GetEgg(1);
            Destroy(collision.gameObject);
        }

        // 물에 입수
        if(collision.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            Debug.Log("물에 접촉");
            // 게임의 처음 위치로 이동
            _playerRigid.velocity = Vector2.zero;
            gameObject.transform.position = GameManager.Instance._startPos;
        }

        // 결승점 도착
        if(collision.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            // 플레이 시간 정지
            GameManager.Instance.StopPlayTime();
        }

    }

    // 플레이어 포톤 뷰 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 동기화 데이터 보내기
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);    // 플레이어의 위치 데이터 전송
            stream.SendNext(transform.rotation);    // 플레이어의 회전 데이터 전송
        }
        else    // 동기화 데이터 받기
        {
            _currentPosition = (Vector3)stream.ReceiveNext();
            _currentRotation = (Quaternion)stream.ReceiveNext();
        }
    }

}
