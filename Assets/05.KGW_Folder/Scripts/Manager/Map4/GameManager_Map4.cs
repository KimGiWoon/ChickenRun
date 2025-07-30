using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;

public class GameManager_Map4 : MonoBehaviourPunCallbacks
{
    [Header("Map4 Ui Manager Reference")]
    [SerializeField] public UIManager_Map4 _gameUIManager;
    [SerializeField] public DrillController _drillController;

    [Header("Map4 Setting")]
    [SerializeField] public float _GamePlayTime;
    [SerializeField] public int _totalPlayerCount;

    static GameManager_Map4 instance;

    int _totalEggCount = 0;
    Map4Data _data;
    public bool _isGoal = false;
    public bool _isFirstPlayer = false;
    public int _goalPlayerCount = 0;
    public Stopwatch _stopwatch;
    public string _currentMapType;
    public float _totalPlayTime;

    // 달걀 획득에 대한 이벤트 (UI 적용)
    public event Action<Map4Data> OnEndGame;

    public static GameManager_Map4 Instance
    {
        get
        {
            if (instance == null)    // 게임매니저가 하이어라키창에 없으면 게임매니저 생성
            {
                GameObject gameObject = new GameObject("GameManager_Map4");
                instance = gameObject.AddComponent<GameManager_Map4>();
            }
            return instance;
        }
    }

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
        // 게임매니저 생성
        CreateGameManager();
        _stopwatch = new Stopwatch();
        _data = new Map4Data("Map4Record");
    }

    private void Start()
    {
        // 총 플레이 인원 설정
        //_totalPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        _totalPlayerCount = 2;

    }

    private void Update()
    {
        // 플레이 타임 오버 체크
        PlayTimeOverCheck();
    }

    // 게임매니저가 생성한게 있으면 생성하지 않고 중복으로 생성 시 삭제
    public void CreateGameManager()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

    // 플레이어 결승점 도착
    public void PlayerReachedGoal(string playerNickname)
    {
        _goalPlayerCount++;
        _data.EggCount = _totalEggCount;
        OnEndGame?.Invoke(_data);
        UnityEngine.Debug.Log($"현재 결승점에 도착한 플레이어 : {_goalPlayerCount}/{_totalPlayerCount}");

        // 모든 플레이어 결승점 도착
        if (_goalPlayerCount >= _totalPlayerCount)
        {
            UnityEngine.Debug.Log("모든 플레이어 도착");
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }
    }

    // 게임 플레이 시간 오버 체크
    private void PlayTimeOverCheck()
    {
        if (_totalPlayTime > _GamePlayTime)
        {
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
        _stopwatch?.Reset();

        // 현재의 방을 나가기
        PhotonNetwork.LeaveRoom();

        // 로비 씬이 있으면 추가해서 씬 이동
        //PhotonNetwork.LoadLevel("로비씬");
    }
}
