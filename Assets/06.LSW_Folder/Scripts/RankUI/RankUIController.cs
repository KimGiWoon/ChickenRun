using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RankUIController: MonoBehaviour
{
    // 랭킹보드 패널 구분을 위한 enum
    private enum PanelType
    {
        Map1,
        Map2,
        Map3,
        Map4,
        Score,
    }
    
    // 랭킹보드 panel만 관리하는 List, RankUI는 따로 관리
    [SerializeField] private List<UIBase> _panelList;
    [SerializeField] private RankUI _rankUI;
    
    private UIBase _currentUI;

    // MVC 패턴을 위한 연동 작업
    private void Awake()
    {
        //ShowUI(PanelType.Map1);
        _rankUI.Initialize(
              onClickMap1Rank: async () =>
              {
                  ShowBoard(PanelType.Map1);
                  await ShowRank("Map1Record");
              },
              onClickMap2Rank: async () =>
              {
                  ShowBoard(PanelType.Map2);
                  await ShowRank("Map2Record");
              },
              onClickMap3Rank: async () =>
              {
                  ShowBoard(PanelType.Map3);
                  await ShowRank("Map3Record");
              },
              onClickMap4Rank: async () =>
              {
                  ShowBoard(PanelType.Map4);
                  await ShowRank("Map4Record");
              },
              onClickScoreRank: async () =>
              {
                  ShowBoard(PanelType.Score);
                  await ShowRank();
              });
    }

    // 버튼 클릭에 따른 랭킹보드 panel change 연동
    private void ShowBoard(PanelType type)
    {
        foreach (var panel in _panelList)
            panel.SetHide();

        _currentUI = _panelList[(int)type];
        _currentUI.SetShow();
    }

    // 유저 개인 랭킹 정보를 Model에서 불러오고 View에 전달하는 메서드
    // 동기 처리를 위한 async - await
    private async Task ShowRank(string record)
    {
        Database_RecordManager.UserRankInfo info = await Database_RecordManager.Instance.LoadUserRank(record);
        if (info.RecordOrScore == null)
        {
            _rankUI.SetPopupText();
            return;
        }
        _rankUI.SetRankText(info.Rank.ToString(), info.Nickname, info.RecordOrScore);
    }
    
    // 유저 개인 랭킹 정보를 Model에서 불러오고 View에 전달하는 메서드(함수 오버로드)
    // 동기 처리를 위한 async - await
    private async Task ShowRank()
    {
        Database_RecordManager.UserRankInfo info = await Database_RecordManager.Instance.LoadUserRank();
        if (info.RecordOrScore == null)
        {
            _rankUI.SetPopupText_Score();
            return;
        }
        _rankUI.SetRankText(info.Rank.ToString(), info.Nickname, info.RecordOrScore);
    }
}
