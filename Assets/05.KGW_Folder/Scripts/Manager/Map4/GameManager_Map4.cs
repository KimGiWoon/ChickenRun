using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager_Map4 : MonoBehaviourPunCallbacks
{
    [Header("Map4 Ui Manager Reference")]
    [SerializeField] public UIManager_Map4 _gameUIManager;
    [SerializeField] public DrillController _drillController;

    [Header("Map4 Setting")]
    [SerializeField] public float _GamePlayTime;

    static GameManager_Map4 instance;

    int _totalEggCount = 0;
    Map4Data _data;
    public bool _isGoal = false;
    public bool _isFirstPlayer = false;
    public int _goalPlayerCount = 0;
    public Stopwatch _stopwatch;
    public string _currentMapType;
    public float _totalPlayTime;
    public int _alivePlayer;
    public int _totalPlayerCount;

    // 달걀 획득에 대한 이벤트 (UI 적용)
    public event Action<int> OnEggCountChange;
    public event Action OnPlayerDeath;
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

    // 달걀 획득
    public void GetEgg(int eggCount)
    {
        _totalEggCount += eggCount;
        OnEggCountChange?.Invoke(_totalEggCount);   // 이벤트 호출
    }

    // 플레이어 죽음
    public void PlayerDeath()
    {
        OnPlayerDeath?.Invoke();    // 카메라 전환 이벤트 호출
        _data.EggCount = _totalEggCount;

        if (PhotonNetwork.IsMasterClient)
        {
            _alivePlayer -= 1;
            UnityEngine.Debug.Log($"살아남은 사람 : {_alivePlayer}");

            if (_alivePlayer <= 0)
            {
                PlayerAllDeath();
            }
        }
    }

    // 플레이어 결승점 도착
    public void PlayerReachedGoal(string playerNickname)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _goalPlayerCount++;
        _data.EggCount = _totalEggCount;
        OnEndGame?.Invoke(_data);
        UnityEngine.Debug.Log($"현재 결승점에 도착한 플레이어 : {_goalPlayerCount}/{_alivePlayer}");

        // 살아남은 플레이어 결승점 도착
        if (_goalPlayerCount >= _alivePlayer)
        {
            UnityEngine.Debug.Log("모든 플레이어 도착");
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }
    }

    // 모든 플레이어가 죽음
    public void PlayerAllDeath()
    {
        UnityEngine.Debug.Log("모든 플레이어가 사망했습니다.");
        photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
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
        //PhotonNetwork.LeaveRoom();

        // 로비 씬이 있으면 추가해서 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }
}
