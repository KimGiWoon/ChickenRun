using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillController : MonoBehaviour
{
    [Header("Drill Move Reference")]
    [SerializeField] float _moveDistance;
    [SerializeField] float _moveSpeed;
    [SerializeField] float _attackDelayTime;
    [SerializeField] GameObject _attackRangeImage;
    [SerializeField] bool _isStart = false;

    Coroutine _drillMoveRoutine;
    Vector2 _moveDirection;
    Vector3 _moveTarget;
    WaitForSeconds _delayTime;

    private void Start()
    {
        Init();
        _drillMoveRoutine = StartCoroutine(DrillAttackCoroutine());
    }

    private void Init()
    {
        GameManager_Map4.Instance._gameUIManager.SetDrillPosition(transform);
        _delayTime = new WaitForSeconds(_attackDelayTime);
        _moveDirection = Vector2.up;
    }

    private IEnumerator DrillAttackCoroutine()
    { 
        // 이동할 위치 세팅
        _moveTarget = transform.position + (Vector3)_moveDirection.normalized * _moveDistance;
        // 이동표시판 활성화
        _attackRangeImage.SetActive(true);

        yield return new WaitForSeconds(10f);

        while (true)
        {
            // 이동표시 비활성화
            _attackRangeImage.SetActive(false);
            // 이동할 위치에 도착할 때까지 반복
            while (Vector3.Distance(transform.position, _moveTarget) > 0.01f)
            {
                transform.position = Vector2.MoveTowards(transform.position, _moveTarget, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 목표위치에 도착
            transform.position = _moveTarget;
            // 이동표시판 활성화
            _attackRangeImage.SetActive(true);

            yield return _delayTime;

            // 다음 이동 위치 설정
            _moveTarget += (Vector3)_moveDirection.normalized * _moveDistance;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("플레이어가 드릴에 닿았습니다.");
        }
    }
}
