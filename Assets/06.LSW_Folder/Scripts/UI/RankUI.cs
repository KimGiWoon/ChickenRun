using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RankUI : UIBase
{
    [SerializeField] private Button _map1RankBtn;
    [SerializeField] private Button _map2RankBtn;
    [SerializeField] private Button _map3RankBtn;
    [SerializeField] private Button _scoreRankBtn;

    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private TextMeshProUGUI _recordOrScore;
    
    // 동기 처리를 위해 Action -> Func<Task>로 변환
    private Func<Task> _onClickMap1Rank;
    private Func<Task> _onClickMap2Rank;
    private Func<Task> _onClickMap3Rank;
    private Func<Task> _onClickScoreRank;

    // async와 void를 동시에 사용하는 것은 좋지 않지만 Unity 이벤트 함수의 한계
    private async void Start()
    {
        _map1RankBtn.onClick.AddListener(() => _onClickMap1Rank?.Invoke());
        _map2RankBtn.onClick.AddListener(() => _onClickMap2Rank?.Invoke());
        _map3RankBtn.onClick.AddListener(() => _onClickMap3Rank?.Invoke());
        _scoreRankBtn.onClick.AddListener(() => _onClickScoreRank?.Invoke());
        
        // 랭킹보드 UI가 켜질 때 버튼이 선택되어있는 것처럼 보이기 위한 시각적 처리
        EventSystem.current.SetSelectedGameObject(_map1RankBtn.gameObject);
        // 랭킹보드 UI가 켜질 때 Map1 관련 데이터가 업데이트되기 위한 기능적 처리
        await _onClickMap1Rank.Invoke();
    }

    // MVC 패턴 구현을 위한 public 메서드
    public void SetRankText(string rank, string nickname, string recordOrScore)
    {
        _rank.text = rank;
        _nickname.text = nickname;
        _recordOrScore.text = recordOrScore;
    }
    
    // MVC 패턴 구현을 위한 public 메서드 (이벤트 처리)
    public void Initialize(
        Func<Task> onClickMap1Rank,
        Func<Task> onClickMap2Rank,
        Func<Task> onClickMap3Rank,
        Func<Task> onClickScoreRank)
    {
        _onClickMap1Rank = onClickMap1Rank;
        _onClickMap2Rank = onClickMap2Rank;
        _onClickMap3Rank = onClickMap3Rank;
        _onClickScoreRank = onClickScoreRank;
    }
    
    // 관련 로직을 UI에 직접적으로 넣기가 애매해서 Controller, Manager 클래스에 위임
    // MVC 패턴 관련 로직은 SetRankText가 처리
    public override void RefreshUI()
    {
        // UserRank 업데이트
    }
}
