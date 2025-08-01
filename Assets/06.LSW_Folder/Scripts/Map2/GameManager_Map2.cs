using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager_Map2 : MonoBehaviour
{
    [SerializeField] private Transform _startPos;
    [SerializeField] private Transform _goalPos;

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
    private float _totalDistance;
    
    public Property<float> GameProgress;
    
    // 달걀 획득에 대한 이벤트 (UI 적용)
    // TODO: UI에서 Start와 OnDestroy에 이벤트 구독과 취소 설정 필요
    public event Action OnReadyGame;
    public event Action<int> OnGetEgg;
    public event Action<Map2Data> OnEndGame;
    public event Action OnTimeUp;
    public event Action<bool> OnPanelOpened;
    
    public class Map2Data
    {
        public string MapType;
        public long Record;
        public int EggCount;

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
        
        // 3분이 지나면 게임 종료
        if (_stopwatch.ElapsedMilliseconds >= 180000)
        {
            StartCoroutine(EndGame());
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
        _stopwatch.Stop();
        _data.Record = _stopwatch.ElapsedMilliseconds;
    }

    // 게임이 종료될 때 호출되는 메서드
    private IEnumerator EndGame()
    {
        _stopwatch.Stop();
        _data.EggCount = _totalEggCount;
        OnEndGame?.Invoke(_data);
        // 게임 결과 UI 호출 
        yield return new WaitForSeconds(3f);
        OnTimeUp?.Invoke();
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
