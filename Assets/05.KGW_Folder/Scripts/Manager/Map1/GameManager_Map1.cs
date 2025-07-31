using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager_Map1 : MonoBehaviourPunCallbacks
{
    [Header("Manager Reference")]
    [SerializeField] public UIManager_Map1 _gameUIManager;
    [SerializeField] NetworkManager_Map1 _networkManager;

    [Header("Map1 Setting")]
    [SerializeField] public float _GamePlayTime;

    static GameManager_Map1 instance;

    int _totalEggCount = 0;
    Map1Data _data;
    public bool _isGoal = false;
    public bool _isFirstPlayer = false;
    public int _goalPlayerCount = 0;
    public Vector3 _startPos;
    public Stopwatch _stopwatch;
    public string _currentMapType;
    public float _totalPlayTime;
    public int _totalPlayerCount;


    // 달걀 획득에 대한 이벤트 (UI 적용)
    public event Action<int> OnEggCountChange;
    public event Action<Map1Data> OnEndGame;

    public static GameManager_Map1 Instance
    {
        get
        {
            if (instance == null)    // 게임매니저가 하이어라키창에 없으면 게임매니저 생성
            {
                instance = FindObjectOfType<GameManager_Map1>();
            }
            return instance;
        }
    }

    // 데이터 베이스에 전달할 맵1 데이터 저장
    public class Map1Data
    {
        public string MapType;
        public long Record;
        public int EggCount;

        public Map1Data(string type)
        {
            MapType = type;
        }
    }

    private void Awake()
    {
        _stopwatch = new Stopwatch();
        _data = new Map1Data("Map1Record");

        // 게임매니저 생성
        CreateGameManager();
    }

    private void Update()
    {
        // 플레이 타임 오버 체크
        PlayTimeOverCheck();
    }

    //게임매니저가 생성한게 있으면 생성하지 않고 중복으로 생성 시 삭제
    public void CreateGameManager()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        if (_stopwatch != null) {
            _stopwatch.Reset();
        }

        // 스탑워치 시작
        _stopwatch.Start();
    }

    // 스탑워치 정지
    public void StopStopWatch()
    {
        _stopwatch.Stop();
        _isGoal = true;
        _data.Record = _stopwatch.ElapsedMilliseconds;
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        _goalPlayerCount++;
        _data.EggCount = _totalEggCount;
        Database_RecordManager.Instance.SaveUserMap1Record(_data);
        OnEndGame?.Invoke(_data);
        UnityEngine.Debug.Log($"현재 결승점에 도착한 플레이어 : {_goalPlayerCount}/{_totalPlayerCount}");

        // 모든 플레이어 결승점 도착
        if (_goalPlayerCount >= _totalPlayerCount) {
            UnityEngine.Debug.Log("모든 플레이어 도착");
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }
    }

    // 게임 플레이 시간 오버 체크
    private void PlayTimeOverCheck()
    {
        if (_totalPlayTime > _GamePlayTime) {
            GamePlayTimeOver();
        }
    }

    // 게임 시간 초과
    public void GamePlayTimeOver()
    {
        _stopwatch.Stop();
        _data.EggCount = _totalEggCount;
        OnEndGame?.Invoke(_data);
        UnityEngine.Debug.Log("게임 플레이 시간이 지났습니다.");
        photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
    }

    // 현재의 방을 나가기
    [PunRPC]
    public void GameClearLeaveRoom()
    {
        UnityEngine.Debug.Log("모든 플레이어가 방을 나갑니다.");
        _networkManager._isStart = false;
        SoundManager.Instance.StopBGM();
        _stopwatch?.Reset();

        // 로비 씬이 있으면 추가해서 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }
}


