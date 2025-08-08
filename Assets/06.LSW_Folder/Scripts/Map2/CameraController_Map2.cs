using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController_Map2 : MonoBehaviour
{
    [Header("Camera Setting Reference")]
    [SerializeField] private float _followSpeed;
    [SerializeField] private bool _isDelayMove;

    private PlayerController_Map2 _followTarget;
    private readonly Vector3 _offset = new Vector3(0, 0, -10f);
    private List<PlayerController_Map2> _observePlayers = new();
    private int _index;
    private bool _isViewing;

    private void Start()
    {
        GameManager_Map2.Instance.OnGoalIn += UpdateList;
    }
    
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
            Debug.Log(_index);
            SetTarget(_observePlayers[_index]);
        }
    }
    
    // 타겟 설정
    public void SetTarget(PlayerController_Map2 target)
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
        Vector3 camPos = _followTarget.transform.position + _offset;
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

    private void UpdateList()
    {
        if (!GameManager_Map2.Instance.Players.Contains(_followTarget))
        {
            _index = 0;
        }
        _observePlayers = GameManager_Map2.Instance.Players;
    }
    
    public void OnViewingMode()
    {
        UpdateList();
        _index = 0;
        if(_observePlayers.Count > 0)
        {
            Debug.Log(_observePlayers.Count);
            SetTarget(_observePlayers[_index]);
            _isViewing = true;
        }
    }
}
