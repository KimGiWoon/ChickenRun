using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController_Map2 : MonoBehaviour
{
    [Header("Camera Setting Reference")]
    [SerializeField] private float _followSpeed;
    [SerializeField] private bool _isDelayMove;

    private Transform _followTarget;
    private readonly Vector3 _offset = new Vector3(0, 0, -10f);

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
}
