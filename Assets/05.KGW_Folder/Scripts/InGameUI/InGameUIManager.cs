using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("Play Time UI Reference")]
    [SerializeField] TMP_Text _playTimeText;

    [Header("Egg Count UI Reference")]
    [SerializeField] TMP_Text _eggCountText;

    // 달걀 획득 UI 이벤트 구독
    private void Start()
    {
        GameManager.Instance.OnEggCountChange += UpdateGetEggUI;
        // 시작할 시 획득한 달걀은 0이므로 UI설정
        UpdateGetEggUI(0);
    }

    private void Update()
    {
        // 플레이 타임 저장
        float playTime = GameManager.Instance._playTime;

        // 시간(분) 설정
        int minuteTime = (int)playTime / 60;
        int secondTime = (int)playTime % 60;

        _playTimeText.text = $"{minuteTime} : {secondTime}";
    }

    // 달걀 획득 UI 이벤트 해제
    private void OnDestroy()
    {
        GameManager.Instance.OnEggCountChange -= UpdateGetEggUI;
    }

    // 달걀 획득 UI
    private void UpdateGetEggUI(int totalEgg)
    {
        _eggCountText.text = $"x {totalEgg}";
    }
}
