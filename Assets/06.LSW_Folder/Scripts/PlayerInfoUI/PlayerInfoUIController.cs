using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInfoUIController : MonoBehaviour
{
    [Header("Drag&Drop")] 
    [SerializeField] private PlayerInfoUI _infoUI;

    // MVC 패턴을 위한 연동
    private void Awake()
    {
        _infoUI.Initialize(onClickExitBtn: () =>
        {
            _infoUI.SetHide();
        });
    }
    
    // 플레이어 정보창이 켜지면 자동 업데이트
    private async void OnEnable()
    {
        await ShowInfo();
    }

    // 플레이어 정보를 model(DatabaseManager)에서 읽어와 view(_infoUI)에 전달
    private async Task ShowInfo()
    {
        var info = await Database_RecordManager.Instance.LoadRankData();
        _infoUI.SetInfoText(info);
    }
}
