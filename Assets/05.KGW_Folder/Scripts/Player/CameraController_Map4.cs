using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController_Map4 : MonoBehaviour
{
    [Header("Camera Setting Reference")]
    [SerializeField] float _followSpeed;
    [SerializeField] public bool _isDelayMove;

    Transform _followTarget;
    Vector3 _offset = new Vector3(0, 2f, -10f);
    List<PlayerController_Map4> _observePlayers = new();
    int _index = 0;
    bool _isViewing = false;

    // 플레이어가 움직이고 난 후 카메라 이동
    private void LateUpdate()
    {
        FollowCamera();
    }

    private void Update()
    {
        // 터치 시 다음 플레이어로 카메라 전환
        if (_isViewing && Input.GetMouseButtonDown(0))
        {
            // 살아있는 플레이어가 없으면 실행 안함
            if (_observePlayers.Count == 0) return;

            // 0번 인덱스에 있는 플레이어 부터 순회하면서 카메라 전환
            _index = (_index + 1) % _observePlayers.Count;
            SetTarget(_observePlayers[_index].transform);
        }
    }

    // 타겟 설정
    public void SetTarget(Transform target)
    {
        _followTarget = target;
    }

    // 타겟 따라가기
    private void FollowCamera()
    {
        if (_followTarget == null)
        {
            return;
        }

        // 카메라 위치 세팅
        Vector3 camPos = _followTarget.position + _offset;
        // 카메라 위치 이동, 딜레이 이동 모드 On
        if (_isDelayMove)
        {
            Vector3 camMovePos = Vector3.Lerp(transform.position, camPos, _followSpeed * Time.deltaTime);
            transform.position = camMovePos;
        }
        else // 즉각 이동 모드 On
        {
            transform.position = camPos;
        }
    }
    
    // 관람 모드
    public void OnViewingMode()
    {
        _observePlayers.Clear();
        _index = 0;

        // 살아 있는 플레이어 순회
        foreach(var player in FindObjectsOfType<PlayerController_Map4>())
        {
            // 결승점에 들어가지 않고 죽지않은 플레이거가 있으면 관찰리스트에 추가
            if (!player._isDeath && !player._isGoal)
            {
                Debug.Log("여기");
                _observePlayers.Add(player);
            }
        }

        if(_observePlayers.Count > 0)
        {
            SetTarget(_observePlayers[_index].transform);
            _isViewing = true;
        }
    }
}
