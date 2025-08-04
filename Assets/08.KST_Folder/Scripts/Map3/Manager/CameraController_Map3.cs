using System.Collections.Generic;
using UnityEngine;

public class CameraController_Map3 : MonoBehaviour
{
    [Header("Camera Setting Reference")]
    [SerializeField] float _followSpeed;
    [SerializeField] public bool _isDelayMove;

    Transform _followTarget;
    Vector3 _offset = new Vector3(0, 2f, -10f);
    List<PlayerController_Map4> _alivePlayers = new();
    int _index = 0;
    bool _isViewing = false;

    // 플레이어가 움직이고 난 후 카메라 이동
    private void LateUpdate()
    {
        FollowCamera();
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
        _alivePlayers.Clear();
        _index = 0;

        // 살아 있는 플레이어 순회
        foreach (var player in FindObjectsOfType<PlayerController_Map4>())
        {
            // 플레이어가 죽지 않았으면
            if (!player._isDeath)
            {
                _alivePlayers.Add(player);
            }
        }

        if (_alivePlayers.Count > 0)
        {
            SetTarget(_alivePlayers[_index].transform);
            _isViewing = true;
        }
    }
}