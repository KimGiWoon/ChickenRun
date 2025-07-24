using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RankUIController: MonoBehaviour
{
    #region Type def

    private enum PanelType
    {
        Map1,
        Map2,
        Map3,
        Score,
    }
    #endregion // Type def

    #region serialized fields
    
    [SerializeField] private List<UIBase> _panelList;
    [SerializeField] private RankUI _rankUI;

    #endregion // serialized fields

    private UIBase _currentUI;
    
    #region mono funcs

    private void Awake()
    {
        //ShowUI(PanelType.Map1);
        _rankUI.Initialize(
              onClickMap1Rank: async () =>
              {
                  ShowUI(PanelType.Map1);
                  await ShowMapRank("Map1Record");
              },
              onClickMap2Rank: async () =>
              {
                  ShowUI(PanelType.Map2);
                  await ShowMapRank("Map2Record");
              },
              onClickMap3Rank: async () =>
              {
                  ShowUI(PanelType.Map3);
                  await ShowMapRank("Map3Record");
              },
              onClickScoreRank: async () =>
              {
                  ShowUI(PanelType.Score);
                  await ShowMapRank();
              });
    }

    #endregion // mono funcs
    
    #region private funcs

    private void ShowUI(PanelType type)
    {
        foreach (var panel in _panelList)
            panel.SetHide();

        _currentUI = _panelList[(int)type];
        _currentUI.SetShow();
    }

    private async Task ShowMapRank(string record)
    {
        Database_RecordManager.UserRankInfo info = await Database_RecordManager.Instance.LoadUserRank(record);
        _rankUI.SetRankText(info.Rank.ToString(), info.Nickname, info.RecordOrScore);
    }
    
    private async Task ShowMapRank()
    {
        Database_RecordManager.UserRankInfo info = await Database_RecordManager.Instance.LoadUserRank();
        _rankUI.SetRankText(info.Rank.ToString(), info.Nickname, info.RecordOrScore);
    }
    
    #endregion // private funcs
}
