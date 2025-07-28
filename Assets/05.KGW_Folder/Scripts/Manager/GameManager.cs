using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stage1 Ui Manager Reference")]
    [SerializeField] public Stage1UIManager _gameUIManager;

    [Header("Stage1 Setting")]
    [SerializeField] public float _GamePlayTime;

    static GameManager instance;
    
    int _totalEggCount = 0;
    public bool _isGoal = false;
    public int _goalPlayerCount = 0;
    public int _totalPlayerCount;
    public Vector3 _startPos;
    public Stopwatch _stopwatch;
    public string _currentMapType;
    public float _totalPlayTime;
    
    // 달걀 획득에 대한 이벤트 (UI 적용)
    public event Action<int> OnEggCountChange;

    public static GameManager Instance
    {
        get
        {
            if(instance == null)    // 게임매니저가 하이어라키창에 없으면 게임매니저 생성
            {
                GameObject gameObject = new GameObject("GameManager");
                instance = gameObject.AddComponent<GameManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 게임매니저 생성
        CreateGameManager();
        _stopwatch = new Stopwatch();
    }

    private void Start()
    {
        // 총 플레이 인원 설정
        //_totalPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        _totalPlayerCount = 2;

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

    // 시작위치 저장
    public void StartPosSave(Transform pos)
    {
        _startPos = pos.position;
    }

    // 스탑워치 시작
    public void StartStopWatch()
    {
        // 스탑워치 리셋
        _stopwatch.Reset();
        // 스탑워치 시작
        _stopwatch.Start();
    }

    // 스탑워치 정지
    public void StopStopWatch()
    {
        _stopwatch.Stop();
        _isGoal = true;

        // 데이터 베이스에 넘겨줄 데이터 저장
        GameResultDate gameResultDate = new GameResultDate
        {
            MapType = _currentMapType,
            Record = (long)_stopwatch.Elapsed.TotalMilliseconds,
            IsWin = _isGoal,
            NormalEgg = _totalEggCount
        };
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
        _goalPlayerCount++;
        UnityEngine.Debug.Log($"현재 결승점에 도착한 플레이어 : {_goalPlayerCount}/{_totalPlayerCount}");

        // 모든 플레이어 결승점 도착
        if(_goalPlayerCount >= _totalPlayerCount)
        {
            UnityEngine.Debug.Log("모든 플레이어 도착");
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }
    }

    // 게임 시간 초과
    public void GamePlayTimeOver()
    {
        _stopwatch.Stop();
        UnityEngine.Debug.Log("게임 플레이 시간이 지났습니다.");
        photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
    }

    // 현재의 방을 나가기
    [PunRPC]
    public void GameClearLeaveRoom()
    {
        UnityEngine.Debug.Log("모든 플레이어가 방을 나갑니다.");

        // 현재의 방을 나가기
        //PhotonNetwork.LeaveRoom(this);

        // 로비 씬이 있으면 추가해서 씬 이동
        //PhotonNetwork.LoadLevel("로비씬");
    }
}

// 파이어 베이스에 넘겨줄 데이터
[Serializable]
public class GameResultDate
{
    public string MapType;
    public long Record;
    public bool IsWin;
    public int NormalEgg;
}

