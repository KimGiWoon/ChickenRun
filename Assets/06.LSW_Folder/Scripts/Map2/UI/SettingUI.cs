using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviourPun
{
    [Header("Option Panel UI Reference")]
    [SerializeField] private GameObject _optionPanel;
    
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Toggle _camModeCheckToggle;
    
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _exitButton;

    private float _cacheBGM;
    private float _cacheSFX;
    
    private void Start()
    {
        InGameUIInit();
        SoundVolumeInit();
    }
    
    private void OnDestroy()
    {
        _backButton.onClick.RemoveListener(OffOptionWindow);
        _exitButton.onClick.RemoveListener(OnExitPlayGame);
    }


    private void InGameUIInit()
    {
        // 카메라 모드 체크 관련 이벤트 구독
        _camModeCheckToggle.onValueChanged.AddListener((isOn) => SettingManager.Instance.SetCamMode(isOn));
        // 사운드 볼륨
        _bgmSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetBGM(volume));
        _sfxSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetSFX(volume));
        
        _backButton.onClick.AddListener(() => OffOptionWindow());
        _exitButton.onClick.AddListener(() => OnExitPlayGame());
    }

    // BGM, SFX 볼륨 초기화
    private void SoundVolumeInit()
    {
        _bgmSlider.value = SettingManager.Instance.BGM.Value;
        _sfxSlider.value = SettingManager.Instance.SFX.Value;
    }
    
    // 옵션 창 닫기
    private void OffOptionWindow()
    {
        _optionPanel.SetActive(false);
        GameManager_Map2.Instance.OpenPanel(false);
    }
    
    // 나가기 버튼 클릭
    private void OnExitPlayGame()
    {
        string exitPlayer = PhotonNetwork.LocalPlayer.CustomProperties["Nickname"].ToString();
        
        // 나감을 알림
        photonView.RPC(nameof(ExitPlayer), RpcTarget.AllViaServer, exitPlayer);
    }
    
    // 방 나가기
    [PunRPC]
    private void ExitPlayer(string playerNickname)
    {
        Debug.Log($"{playerNickname}께서 나갔습니다.");
        if (PhotonNetwork.LocalPlayer.CustomProperties["Nickname"].ToString() == playerNickname)
        {
            GameManager_Map2.Instance.GameDefeatLeaveRoom();
            PhotonNetwork.LoadLevel("MainScene");
            PhotonNetwork.LeaveRoom();
        }
    }
}
