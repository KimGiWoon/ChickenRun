using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class GameManager_Map1 : MonoBehaviourPunCallbacks
{
    [Header("Manager Reference")]
    [SerializeField] UIManager_Map1 _gameUIManager;
    [SerializeField] NetworkManager_Map1 _networkManager;

    [Header("Map1 Setting")]
    [SerializeField] public float _GamePlayTime;
    [SerializeField] ClearUIController_Map1 _clearUIController;
    [SerializeField] DefeatUIController_Map1 _defeatUIController;
    [SerializeField] TimeOverUIController_Map1 _timeOverUIController;
    [SerializeField] public CameraController_Map1 _cameraController;

    int _totalEggCount = 0;
    MapData _data;
    public bool _isFirstPlayer = false;
    public bool _isGameOver = false;
    public Vector3 _startPos;
    public Stopwatch _stopwatch;
    public string _currentMapType;
    public float _totalPlayTime;
    public int _totalPlayerCount;
    public int _goalPlayerCount = 0;
    public int _exitPlayerCount = 0;

    // 달걀 획득에 대한 이벤트 (UI 적용)
    public event Action<int> OnEggCountChange;
    public event Action OnPlayerGoal;

    private void Awake()
    {
        _stopwatch = new Stopwatch();
        _data = new MapData("Map1Record");
        PhotonNetwork.RunRpcCoroutines = true;
    }

    private void Update()
    {
        // 플레이 타임 오버 체크
        PlayTimeOverCheck();
    }

    // 시작위치 저장
    public void StartPosSave(Transform pos)
    {
        _startPos = pos.position;
    }

    // 스탑워치 시작
    public void StartStopWatch()
    {
        // 스탑워치 리셋
        if (_stopwatch != null) 
        {
            _stopwatch.Reset();
        }

        // 스탑워치 시작
        _stopwatch.Start();
    }

    // 스탑워치 정지
    public void StopStopWatch()
    {
        _stopwatch.Stop();
    }

    // 플레이 타임 UI 업데이트
    public string PlayTimeUpdate()
    {
        float arrivalTime = (float)_stopwatch.Elapsed.TotalSeconds;
        int minuteTime = (int)arrivalTime / 60;
        float secondTime = arrivalTime % 60;

        // 플레이 시간 저장
        _totalPlayTime = arrivalTime;

        return string.Format($"{minuteTime:D2}:{secondTime:00.00}");
    }

    // 달걀 획득
    public void GetEgg(int eggCount)
    {
        _totalEggCount += eggCount;
        OnEggCountChange?.Invoke(_totalEggCount);   // 이벤트 호출
    }

    // 플레이어 결승점 도착
    public void PlayerReachedGoal(string playerNickname)
    {
        if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Nickname", out object nickname))
        {
            string myNickname = nickname.ToString();

            // 내 닉네임과 골인한 유저의 닉네임 확인
            if(myNickname == playerNickname)
            {
                UnityEngine.Debug.Log(playerNickname);

                // 골인하면 카메라 전환 이벤트 호출
                OnPlayerGoal?.Invoke();

                // 랭킹 데이터 저장
                _data.EggCount = _totalEggCount;
                _data.Record = _stopwatch.ElapsedMilliseconds;

                // 점수, 달걀, 시간 저장
                Database_RecordManager.Instance.SaveUserMapRecord(_data);
            }
        }       

        if (PhotonNetwork.IsMasterClient)
        {
            _goalPlayerCount++;

            CheckGameEndCondition();
        }       
    }

    // 게임 종료 조건 체크
    public void CheckGameEndCondition()
    {
        int currentPlayerCount = _totalPlayerCount - _exitPlayerCount;

        // 플레이어 상태 체크
        if (_goalPlayerCount >= currentPlayerCount)
        {
            // 게임 클리어를 진행한 플레이어가 없으면
            if (_goalPlayerCount <= 0)
            {
                photonView.RPC(nameof(GameDefeatLeaveRoom), RpcTarget.All);
            }
            else    // 게임 클리어를 진행한 플레이어가 있으면
            {
                photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.All);
            }
        }
    }

    // 게임 플레이 시간 오버 체크
    private void PlayTimeOverCheck()
    {
        if (_isGameOver) return;

        if (_totalPlayTime >= _GamePlayTime) 
        {
            _isGameOver = true;
            GamePlayTimeOver();
        }
    }

    // 게임 시간 초과
    public void GamePlayTimeOver()
    {
        _stopwatch.Stop();
        photonView.RPC(nameof(GameTimeOverLeaveRoom), RpcTarget.AllViaServer);
    }

    // 클리어 후 현재의 방을 나가기
    [PunRPC]
    public void GameClearLeaveRoom()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        _networkManager._isStart = false;
        _gameUIManager.ClearPlayerReference();
        
        // 클리어 UI 활성화
        _clearUIController.gameObject.SetActive(true);
    }

    // 실패 후 현재의 방을 나가기
    [PunRPC]
    public void GameDefeatLeaveRoom()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        _networkManager._isStart = false;
        _gameUIManager.ClearPlayerReference();

        // 실패 UI 활성화
        _defeatUIController.gameObject.SetActive(true);
    }

    // 타임오버 후 현재의 방을 나가기
    [PunRPC]
    public void GameTimeOverLeaveRoom()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        _networkManager._isStart = false;
        _gameUIManager.ClearPlayerReference();

        // 클리어 UI 활성화
        _timeOverUIController.gameObject.SetActive(true);
    }
}


