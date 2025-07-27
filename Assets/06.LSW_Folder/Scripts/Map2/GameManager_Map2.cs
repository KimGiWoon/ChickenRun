using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;

public class GameManager_Map2 : Singleton<GameManager_Map2>
{
    private Vector3 _startPos;
    private Stopwatch _stopwatch;
    private Map2Data _data;
    private int _totalEggCount;
    private bool _isEnd;

    public bool IsGoal { get; private set; }
    
    
    // 달걀 획득에 대한 이벤트 (UI 적용)
    // TODO: UI에서 Start와 OnDestroy에 이벤트 구독과 취소 설정 필요
    public event Action<int> OnGetEgg;
    public event Action<Map2Data> OnEndGame;
    public event Action OnTimeUp;
    
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

    protected override void Awake()
    {
        base.Awake();
        _stopwatch = new Stopwatch();
        _data = new Map2Data("Map2Record");
    }

    private void Update()
    {
        if (_stopwatch.ElapsedMilliseconds >= 180000)
        {
            StartCoroutine(EndGame());
        }
    }
    
    public void StartGame()
    {
        _stopwatch.Start();
    }
    
    // 결승점 도착
    public void ReachGoalPoint()
    {
        IsGoal = true;
        _stopwatch.Stop();
        _data.Record = _stopwatch.ElapsedMilliseconds;
    }

    public IEnumerator EndGame()
    {
        _stopwatch.Stop();
        _data.EggCount = _totalEggCount;
        OnEndGame?.Invoke(_data);
        // 게임 결과 UI 호출 
        yield return new WaitForSeconds(3f);
        _stopwatch.Reset();
        OnTimeUp?.Invoke();
    }
    
    // 달걀 획득
    public void GetEgg()
    {
        _totalEggCount++;
        OnGetEgg?.Invoke(_totalEggCount);   // 이벤트 호출
        Debug.Log($"달걀을 획득 했습니다. 총 획득 달걀 : {_totalEggCount}");
    }
}
