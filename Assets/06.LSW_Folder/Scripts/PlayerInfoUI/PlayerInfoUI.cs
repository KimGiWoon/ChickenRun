using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : UIBase
{
    [SerializeField] private Button _exitBtn;
    
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private TextMeshProUGUI _map1Record;
    [SerializeField] private TextMeshProUGUI _map2Record;
    [SerializeField] private TextMeshProUGUI _map3Record;
    [SerializeField] private TextMeshProUGUI _score;

    private Action _onClickExitBtn;

    // 내부 이벤트 처리는 Awake에서 진행
    private void Awake()
    {
        _exitBtn.onClick.AddListener(() => _onClickExitBtn?.Invoke());
    }
    
    // MVC 패턴을 위한 연동 메서드
    public void Initialize(Action onClickExitBtn)
    {
        _onClickExitBtn = onClickExitBtn;
    }
    
    // Controller에서 받은 정보로 UI(Text) 업데이트 메서드
    public void SetInfoText(Database_RecordManager.RankData data)
    {
        _nickname.text = data.Nickname;
        _map1Record.text = $"Map1 : {Database_RecordManager.Instance.FormatData((int)data.Map1Record)}";
        _map2Record.text = $"Map2 : {Database_RecordManager.Instance.FormatData((int)data.Map2Record)}";
        _map3Record.text = $"Map3 : {Database_RecordManager.Instance.FormatData((int)data.Map3Record)}";
        _score.text = $"Score : {data.Score.ToString()}";
    }
}
