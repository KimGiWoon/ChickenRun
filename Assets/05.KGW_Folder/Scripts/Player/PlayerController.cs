using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player State Reference")]
    [SerializeField] PlayerState _playerstate;

    Rigidbody2D _playerRigid;
    Vector2 _jumpDir;
    float _touchStartTime;
    float _touchEndTime;
    bool _isGround;
    bool _isTouch;

    private void Awake()
    {
        _playerRigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // 플레이어 시작위치 저장
        GameManager.Instance.StartPosSave(transform);
    }

    private void Update()
    {
        TouchInput();
        PlayerJump();
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
}
