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
    
    private Func<Task> _onClickMap1Rank;
    private Func<Task> _onClickMap2Rank;
    private Func<Task> _onClickMap3Rank;
    private Func<Task> _onClickScoreRank;

    private async void Start()
    {
        _map1RankBtn.onClick.AddListener(() => _onClickMap1Rank?.Invoke());
        _map2RankBtn.onClick.AddListener(() => _onClickMap2Rank?.Invoke());
        _map3RankBtn.onClick.AddListener(() => _onClickMap3Rank?.Invoke());
        _scoreRankBtn.onClick.AddListener(() => _onClickScoreRank?.Invoke());
        
        EventSystem.current.SetSelectedGameObject(_map1RankBtn.gameObject);

        await _onClickMap1Rank.Invoke();
    }

    public void SetRankText(string rank, string nickname, string recordOrScore)
    {
        _rank.text = rank;
        _nickname.text = nickname;
        _recordOrScore.text = recordOrScore;
    }
    
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
    
    public override void RefreshUI()
    {
        // todo : UserRank 업데이트
    }
}
