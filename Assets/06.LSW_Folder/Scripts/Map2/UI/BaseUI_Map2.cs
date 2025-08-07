using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseUI_Map2 : MonoBehaviourPun
{
    [Header("Text UI Reference")]
    [SerializeField] TMP_Text _playTimeText;
    [SerializeField] TMP_Text _eggCountText;
    [SerializeField] TMP_Text _endTimeText;

    [Header("Button UI Reference")]
    [SerializeField] Button _optionButton;
    [SerializeField] Button _emoticonButton;

    [Header("Panel UI Reference")]
    [SerializeField] GameObject _optionPanel;
    [SerializeField] GameObject _emoticonPanel;
    [SerializeField] GameObject _defeatPanel;
    [SerializeField] GameObject _winPanel;
    [SerializeField] GameObject _endTimePanel;
    
    [Header("Goal Slider UI Reference")]
    [SerializeField] Slider _playerPosSlider;
    
    private bool _isEmoticonPanelOpen;
    private bool _isSettingPanelOpen;

    private Coroutine _endTimerRoutine;
    private readonly int _timer = 60;

    private void Start()
    {
        // 달걀 획득 UI 이벤트 구독
        GameManager_Map2.Instance.OnGetEgg += UpdateEggText;
        GameManager_Map2.Instance.GameProgress.OnChanged += UpdateSlider;
        GameManager_Map2.Instance.OnWinGame += () => _winPanel.SetActive(true);
        GameManager_Map2.Instance.OnDefeatGame += () => _defeatPanel.SetActive(true);
        GameManager_Map2.Instance.OnReachGoal += () =>
        {
            photonView.RPC(nameof(EndTimer), RpcTarget.AllViaServer);
        };
        
        _optionButton.onClick.AddListener(OnOptionPanel);
        _emoticonButton.onClick.AddListener(OnEmoticonPanel);
        // 시작할 시 획득한 달걀은 0이므로 UI설정
        UpdateEggText(0);
    }

    private void Update()
    {
        // 플레이 타임 UI 출력
        _playTimeText.text = GameManager_Map2.Instance.PlayTimeUpdate();
    }

    private void OnDestroy()
    {
        // 달걀 획득 UI 이벤트 해제
        if (Application.isPlaying)
        {
            //GameManager_Map2.Instance.OnGetEgg -= UpdateEggText;
            //GameManager_Map2.Instance.GameProgress.OnChanged -= UpdateSlider;
        
            _optionButton.onClick.RemoveListener(OnOptionPanel);
            _emoticonButton.onClick.RemoveListener(OnEmoticonPanel);
        }
    }

    // 달걀 획득 UI 업데이트
    private void UpdateEggText(int totalEgg)
    {
        _eggCountText.text = $"x {totalEgg}";
    }

    // 옵션 패널 오픈
    private void OnOptionPanel()
    {
        if (!_isSettingPanelOpen)
        {
            _optionPanel.SetActive(true);
            _isSettingPanelOpen = true;
        }
        else
        {
            _optionPanel.SetActive(false);
            _isSettingPanelOpen = false;
        }
        GameManager_Map2.Instance.OpenPanel(_isSettingPanelOpen);
    }
    
    // 이모티콘 패널 오픈
    private void OnEmoticonPanel()
    {
        if (!_isEmoticonPanelOpen)
        {
            _emoticonPanel.SetActive(true);
            _isEmoticonPanelOpen = true;
        }
        else
        {
            _emoticonPanel.SetActive(false);
            _isEmoticonPanelOpen = false;
        }
        GameManager_Map2.Instance.OpenPanel(_isEmoticonPanelOpen);
    }
    
    // 플레이어 위치 업데이트
    private void UpdateSlider(float value)
    {
        _playerPosSlider.value = value;
    }

    [PunRPC]
    public void EndTimer()
    {
        _endTimerRoutine = StartCoroutine(EndTimerCoroutine());
    }
    
    private IEnumerator EndTimerCoroutine()
    {
        WaitForSeconds time = new WaitForSeconds(1f);
        _endTimePanel.SetActive(true);

        for(int i = _timer; i >= 1; i--)
        {
            _endTimeText.text = i.ToString();
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Count);

            yield return time;
        }
        _endTimePanel.SetActive(false);
        _endTimerRoutine = null;
    }
}

