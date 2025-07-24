using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    int _totalEggCount = 0;
    bool _isGoal = false;
    public Vector3 _startPos;
    public float _playTime = 0f;

    // 달걀 획득에 대한 이벤트 (UI 적용)
    // TODO: Egg UI에서 Start와 OnDestroy에 이벤트 구독과 취소 설정 필요
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
    }

    private void Update()
    {
        // 결승점 도착 까지 플레이 시간 측정
        if(!_isGoal)
        {
            _playTime += Time.deltaTime;
        }
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

    // 결승점 도착
    public void StopPlayTime()
    {
        _isGoal = true;
    }

    // 달걀 획득
    public void GetEgg(int eggCount)
    {
        _totalEggCount += eggCount;
        OnEggCountChange?.Invoke(_totalEggCount);   // 이벤트 호출
        Debug.Log($"달걀을 {eggCount}개 획득을 했습니다. 총 획득 달걀 : {_totalEggCount}");
    }

}
