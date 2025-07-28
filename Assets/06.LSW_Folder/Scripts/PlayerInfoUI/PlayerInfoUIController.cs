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
            _infoUI.ResetText();
            _infoUI.SetHide();
        });
    }

    // 외부 메서드 이벤트 연결은 Start 처리
    private void Start()
    {
        UserPersonalRecord.OnClickPlayerInfo += async (uid) => await ShowInfo(uid);
        _infoUI.ResetText();
        _infoUI.SetHide();
    }

    // uid에 해당하는 플레이어 정보를 model(DatabaseManager)에서 읽어와 view(_infoUI)에 전달
    private async Task ShowInfo(string uid)
    {
        _infoUI.gameObject.SetActive(true);
        var info = await Database_RecordManager.Instance.LoadRankData(uid);
        _infoUI.SetInfoText(info);
    }
}
