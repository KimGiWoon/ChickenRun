using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager_Map2 : MonoBehaviourPun
{
    [SerializeField] private Transform _startPos;
    [SerializeField] private Transform _goalPos;
    [SerializeField] private float _gamePlayTime = 600f;
    private readonly float _gameExtraTime = 60f;
    
    private static GameManager_Map2 _instance;
    public static GameManager_Map2 Instance
    {
        get
        {
            return _instance;
        }
    }

    private List<PlayerController_Map2> _players;
    public List<PlayerController_Map2> Players => _players;
    
    private Transform _player;
    private Stopwatch _stopwatch;
    private MapData _data;
    private int _totalEggCount;
    private bool _isEnd;
    private bool _isGameOver;
    private bool _isWin;
    private bool _isLose;
    private bool _isReach;
    public bool IsLose
    {
        get { return _isLose; }
    }
    private float _totalDistance;
    private float _totalPlayTime;
    
    public Property<float> GameProgress;
    
    // 달걀 획득에 대한 이벤트 (UI 적용)
    // TODO: UI에서 Start와 OnDestroy에 이벤트 구독과 취소 설정 필요
    public event Action OnReadyGame;
    public event Action OnGoalIn;
    public event Action OnWinGame;
    public event Action OnDefeatGame;
    public event Action OnReachGoal;
    public event Action<int> OnGetEgg;
    public event Action<bool> OnPanelOpened;
    

    public void Awake()
    {
        Init();
        _players = new List<PlayerController_Map2>();
        _stopwatch = new Stopwatch();
        _data = new MapData("Map2Record");
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
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            foreach(var player in FindObjectsOfType<PlayerController_Map2>())
            {
                // 결승점에 들어가지 않은 플레이거가 있으면 관찰리스트에 추가
                if (!_players.Contains(player))
                {
                    _players.Add(player);
                }
            }
        }
    }
    
    // 게임 시작
    public void StartGame()
    {
        _stopwatch.Start();
    }
    
    // 결승점 도착
    public void ReachGoalPoint(string team)
    {
        _data.Record = _stopwatch.ElapsedMilliseconds;
        _isEnd = true;

        if (!_isReach)
        {
            OnReachGoal?.Invoke();
            photonView.RPC(nameof(FirstReach), RpcTarget.All);
            
            if (_isLose) return;
            photonView.RPC(nameof(LoseGame), RpcTarget.Others, team);
            _gamePlayTime = _totalPlayTime;
            _data.IsWin = true;
            _data.EggCount = 50;
        }
    }

    
    public void GoalInPlayer(PlayerController_Map2 player)
    {
        _players.Remove(player);
        OnGoalIn?.Invoke();
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
            Database_RecordManager.Instance.SaveUserMapRecord(_data);
            Debug.Log("기록이 저장되었습니다.");
        }
        Debug.Log("모든 플레이어가 방을 나갑니다.");
        if (_isLose)
        {
            GameDefeatLeaveRoom();
        }
        else
        {
            GameWinLeaveRoom();
        }
    }

    [PunRPC]
    public void LoseGame(string team)
    {
        _isLose = true;

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Color", out var myTeam) && myTeam.ToString() == team)
        {
            _data.IsWin = true;
            _data.EggCount = 50;
        }
    }
    
    [PunRPC]
    public void FirstReach()
    {
        _isReach = true;
    }

    private void GameWinLeaveRoom()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();
        
        OnWinGame?.Invoke();
    }
    
    public void GameDefeatLeaveRoom()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();
        
        OnDefeatGame?.Invoke();
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
