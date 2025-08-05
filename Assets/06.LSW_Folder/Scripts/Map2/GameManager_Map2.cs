using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager_Map2 : MonoBehaviourPun
{
    [SerializeField] private Transform _startPos;
    [SerializeField] private Transform _goalPos;
    [SerializeField] private float _gamePlayTime = 600f;
    [SerializeField] private float _gameExtraTime = 5f;
    
    private static GameManager_Map2 _instance;
    public static GameManager_Map2 Instance
    {
        get
        {
            return _instance;
        }
    }
    
    private Transform _player;
    private Stopwatch _stopwatch;
    private Map2Data _data;
    private int _totalEggCount;
    private bool _isEnd;
    private bool _isGameOver;
    private bool _isWin;
    private bool _isLose;
    private float _totalDistance;
    private float _totalPlayTime;
    
    public Property<float> GameProgress;
    
    // 달걀 획득에 대한 이벤트 (UI 적용)
    // TODO: UI에서 Start와 OnDestroy에 이벤트 구독과 취소 설정 필요
    public event Action OnReadyGame;
    public event Action OnReachGoal;
    public event Action<int> OnGetEgg;
    public event Action<bool> OnPanelOpened;
    
    public class Map2Data
    {
        public string MapType;
        public long Record;
        public int EggCount;
        public bool IsWin;

        public Map2Data(string type)
        {
            MapType = type;
        }
    }

    public void Awake()
    {
        Init();
        _stopwatch = new Stopwatch();
        _data = new Map2Data("Map2Record");
        GameProgress = new Property<float>(0f);
        SetTotalDistance();
    }

    private void Update()
    {
        // 플레이어와 결승선의 거리 확인
        PlayerPosUpdate();
        
        if (_isGameOver) return;

        if (_totalPlayTime >= _gamePlayTime + _gameExtraTime) 
        {
            _isGameOver = true;
            GamePlayTimeOver();
        }
    }

    private void Init()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else 
        { 
            Destroy(gameObject);
        }
    }
    
    public void OpenPanel(bool isOpen)
    {
        OnPanelOpened?.Invoke(isOpen);
    }
    
    public string PlayTimeUpdate()
    {
        float arrivalTime = (float)_stopwatch.Elapsed.TotalSeconds;
        int minuteTime = (int)arrivalTime / 60;
        float secondTime = arrivalTime % 60;
        
        _totalPlayTime = arrivalTime;

        return string.Format($"{minuteTime:D2}:{secondTime:00.00}");
    }

    public void ReadyGame()
    {
        OnReadyGame?.Invoke();
    }
    
    // 게임 시작
    public void StartGame()
    {
        _stopwatch.Start();
    }
    
    // 결승점 도착
    public void ReachGoalPoint()
    {
        _data.Record = _stopwatch.ElapsedMilliseconds;
        Debug.Log(_data.Record);
        _isEnd = true;
        OnReachGoal?.Invoke();
        if (!_isLose)
        {
            photonView.RPC(nameof(LoseGame), RpcTarget.AllViaServer);
            _gamePlayTime = _totalPlayTime;
            _data.IsWin = true;
            Debug.Log("1분 뒤에 게임이 종료됩니다.");
        }
    }

    private void GamePlayTimeOver()
    {
        /*_stopwatch.Stop();
        UnityEngine.Debug.Log("게임 플레이 시간이 지났습니다.");
        if (_isEnd)
        {
            Database_RecordManager.Instance.SaveUserMap2Record(_data);
            Debug.Log("기록이 저장되었습니다.");
        }*/
        photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
    }

    // 현재의 방을 나가기
    [PunRPC]
    public void GameClearLeaveRoom()
    {
        _stopwatch.Stop();
        Debug.Log("게임 플레이 시간이 지났습니다.");
        if (_isEnd)
        {
            Database_RecordManager.Instance.SaveUserMap2Record(_data);
            Debug.Log("기록이 저장되었습니다.");
        }
        Debug.Log("모든 플레이어가 방을 나갑니다.");
        //PhotonNetwork.LeaveRoom();
        // 로비 씬이 있으면 추가해서 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();
    }

    [PunRPC]
    public void LoseGame()
    {
        _isLose = true;
    }
    
    public void GameDefeatLeaveRoom()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        // 실패 UI 활성화
        //_defeatUIController.gameObject.SetActive(true);
    }
    
    // 달걀 획득
    public void GetEgg()
    {
        _totalEggCount++;
        OnGetEgg?.Invoke(_totalEggCount);
    }
    
    // 출발지점과 도착지점 위치 확인
    private void SetTotalDistance()
    {
        _totalDistance = Vector2.Distance(_startPos.position, _goalPos.position);
    }

    // 플레이어의 위치와 거리 업데이트
    private void PlayerPosUpdate()
    {
        if (_player == null) return;
        float distanceToGoal = Vector2.Distance(_player.position, _goalPos.position);
        GameProgress.Value = Mathf.Clamp01(1 - (distanceToGoal / _totalDistance));
    }

    // 플레이어 위치 세팅
    public void SetPlayer(Transform player)
    {
        _player = player;
    }
}
