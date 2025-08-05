using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankUI : UIBase
{
    [SerializeField] private Toggle _map1RankToggle;
    [SerializeField] private Toggle _map2RankToggle;
    [SerializeField] private Toggle _map3RankToggle;
    [SerializeField] private Toggle _map4RankToggle;
    [SerializeField] private Toggle _scoreRankToggle;

    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _normalSprite;

    [SerializeField] private GameObject _userRankRecord;
    [SerializeField] private GameObject _userRankEmpty;
    
    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private TextMeshProUGUI _recordOrScore;
    [SerializeField] private TextMeshProUGUI _popup;
    
    // 동기 처리를 위해 Action -> Func<Task>로 변환
    private Func<Task> _onClickMap1Rank;
    private Func<Task> _onClickMap2Rank;
    private Func<Task> _onClickMap3Rank;
    private Func<Task> _onClickMap4Rank;
    private Func<Task> _onClickScoreRank;

    // async와 void를 동시에 사용하는 것은 좋지 않지만 Unity 이벤트 함수의 한계
    // 버튼을 선택하고 나서 UI 다른 곳을 터치했을 때 sprite가 normal 상태로 돌아가는 문제
    // 버튼을 토글로 바꾸고 isOn을 매개변수로 받아 이를 토대로 sprite를 코드로 관리하여 해결 
    private async void Start()
    {
        _map1RankToggle.onValueChanged.AddListener((isOn) =>
        {
            _map1RankToggle.image.sprite = isOn ? _selectedSprite : _normalSprite;
            if (isOn)
            {
                _onClickMap1Rank?.Invoke();
            }
        });
        _map2RankToggle.onValueChanged.AddListener((isOn) => 
        {
            _map2RankToggle.image.sprite = isOn ? _selectedSprite : _normalSprite;
            if (isOn)
            {
                _onClickMap2Rank?.Invoke();
            }
        });
        _map3RankToggle.onValueChanged.AddListener((isOn) => 
        {
            _map3RankToggle.image.sprite = isOn ? _selectedSprite : _normalSprite;
            if (isOn)
            {
                _onClickMap3Rank?.Invoke();
            }
        });
        _map4RankToggle.onValueChanged.AddListener((isOn) => 
        {
            _map4RankToggle.image.sprite = isOn ? _selectedSprite : _normalSprite;
            if (isOn)
            {
                _onClickMap4Rank?.Invoke();
            }
        });
        _scoreRankToggle.onValueChanged.AddListener((isOn) => 
        {
            _scoreRankToggle.image.sprite = isOn ? _selectedSprite : _normalSprite;
            if (isOn)
            {
                _onClickScoreRank?.Invoke();
            }
        });
        // Button일 때 시각적 처리 코드
        //EventSystem.current.SetSelectedGameObject(_map1RankToggle.gameObject);
        
        // 랭킹보드 UI가 켜질 때 버튼이 선택되어있는 것처럼 보이기 위한 시각적 처리
        _map1RankToggle.image.sprite = _selectedSprite;
        // 랭킹보드 UI가 켜질 때 Map1 관련 데이터가 업데이트되기 위한 기능적 처리
        await _onClickMap1Rank.Invoke();
    }

    // MVC 패턴 구현을 위한 public 메서드
    public void SetRankText(string rank, string nickname, string recordOrScore)
    {
        _userRankRecord.SetActive(true);
        _userRankEmpty.SetActive(false);
        _rank.text = rank;
        _nickname.text = nickname;
        _recordOrScore.text = recordOrScore;
    }
    
    // 기록이 없는 경우 플레이 안내 팝업 텍스트 출력 메서드
    // 예외 처리 로직은 Controller 내에서 수행
    public void SetPopupText()
    {
        _userRankRecord.SetActive(false);
        _userRankEmpty.SetActive(true);
        _popup.text = "플레이하여 기록을 세워보세요!";
    }
    
    public void SetPopupText_Score()
    {
        _userRankRecord.SetActive(false);
        _userRankEmpty.SetActive(true);
        _popup.text = "멀티 게임에서 승리를 해보세요!";
    }
    
    // MVC 패턴 구현을 위한 public 메서드 (이벤트 처리)
    public void Initialize(
        Func<Task> onClickMap1Rank,
        Func<Task> onClickMap2Rank,
        Func<Task> onClickMap3Rank,
        Func<Task> onClickMap4Rank,
        Func<Task> onClickScoreRank)
    {
        _onClickMap1Rank = onClickMap1Rank;
        _onClickMap2Rank = onClickMap2Rank;
        _onClickMap3Rank = onClickMap3Rank;
        _onClickMap4Rank = onClickMap4Rank;
        _onClickScoreRank = onClickScoreRank;
    }
    
    // 관련 로직을 UI에 직접적으로 넣기가 애매해서 Controller, Manager 클래스에 위임
    // MVC 패턴 관련 로직은 SetRankText가 처리
    public override void RefreshUI()
    {
        // UserRank 업데이트
    }
}
