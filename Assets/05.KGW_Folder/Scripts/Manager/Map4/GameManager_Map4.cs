using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager_Map4 : MonoBehaviourPunCallbacks
{
    [Header("Manager Reference")]
    [SerializeField] public UIManager_Map4 _gameUIManager;
    [SerializeField] public DrillController _drillController;
    [SerializeField] NetworkManager_Map4 _networkManager;

    [Header("Map4 Setting")]
    [SerializeField] public float _GamePlayTime;
    [SerializeField] ClearUIController_Map4 _clearUIController;
    [SerializeField] DefeatUIController_Map4 _defeatUIController;
    [SerializeField] PlayerController_Map4 _playerController;
    [SerializeField] public CameraController_Map4 _cameraController;

    int _totalEggCount = 0;
    Map4Data _data;
    public bool _isFirstPlayer = false;
    public Stopwatch _stopwatch;
    public string _currentMapType;
    public float _totalPlayTime;
    public int _alivePlayer;
    public int _totalPlayerCount;
    public int _goalPlayerCount = 0;
    public int _deathPlayerCount = 0;
    public int _exitPlayerCount = 0;

    // 달걀 획득에 대한 이벤트 (UI 적용)
    public event Action<int> OnEggCountChange;
    public event Action OnPlayerDeath;
    public event Action OnPlayerGoal;
    public event Action<Map4Data> OnEndGame;

    // 데이터 베이스에 전달할 맵1 데이터 저장
    public class Map4Data
    {
        public string MapType;
        public long Record;
        public int EggCount;

        public Map4Data(string type)
        {
            MapType = type;
        }
    }

    private void Awake()
    {
        _stopwatch = new Stopwatch();
        _data = new Map4Data("Map4Record");
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

    // 플레이어 탈주
    public void PlayerExit(string exitPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _exitPlayerCount++;
            UnityEngine.Debug.Log($"나간 사람 수 : {_exitPlayerCount}");
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Nickname", out object nickname))
        {
            string myNickname = nickname.ToString();

            if (exitPlayer == myNickname)
            {
                UnityEngine.Debug.Log($"{exitPlayer}께서 나갔습니다.");

                _networkManager._isStart = false;
                _defeatUIController.gameObject.SetActive(true);
            }
        }
    }

    // 플레이어 죽음
    public void PlayerDeath(string playerNickname)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Nickname", out object nickname))
        {
            string myNickname = nickname.ToString();

            if (myNickname == playerNickname)
            {
                OnPlayerDeath?.Invoke();
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            _deathPlayerCount++;

            // 모든 플레이어가 죽음
            if (_exitPlayerCount + _goalPlayerCount + _deathPlayerCount >= _totalPlayerCount)
            {
                photonView.RPC(nameof(GameDefeatLeaveRoom), RpcTarget.AllViaServer);
            }
        }
    }

    // 플레이어 결승점 도착
    public void PlayerReachedGoal(string playerNickname)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Nickname", out object nickname))
        {
            string myNickname = nickname.ToString();

            if (myNickname == playerNickname)
            {
                UnityEngine.Debug.Log(playerNickname);
                OnPlayerGoal?.Invoke();
                SoundManager.Instance.StopBGM();
                _data.EggCount = _totalEggCount;
                _data.Record = _stopwatch.ElapsedMilliseconds;
                OnEndGame?.Invoke(_data);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            _goalPlayerCount++;

            if (_exitPlayerCount + _goalPlayerCount + _deathPlayerCount >= _totalPlayerCount)
            {
                photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
            }
        }
    }

    // 클리어 후 현재의 방을 나가기
    [PunRPC]
    public void GameClearLeaveRoom()
    {
        _networkManager._isStart = false;
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        // 클리어 UI 활성화
        _clearUIController.gameObject.SetActive(true);
    }

    // 실패 후 현재의 방을 나가기
    [PunRPC]
    public void GameDefeatLeaveRoom()
    {
        _networkManager._isStart = false;
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        // 실패 UI 활성화
        _defeatUIController.gameObject.SetActive(true);
    }
}
