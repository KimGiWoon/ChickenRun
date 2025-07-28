using System;
using System.Threading.Tasks;
using UnityEngine;

public class PopupManager : Singleton<PopupManager>
{
    #region Singleton

    protected override void Awake()
    {
        base.Awake();
    }

    #endregion // Singleton





    #region serialized Fields

    [SerializeField] private PopupPanel _popupPanel;
    [SerializeField] private PlayerInfoUI _infoUI;

    #endregion // serialized Fields





    #region public funcs

    /// <summary>
    /// 확인 취소 버튼 두개가 있는 팝업을 보여줍니다.
    /// </summary>
    /// <param name="message">팝업 가운데에 띄울 메세지 내용입니다.</param>
    /// <param name="leftText">왼쪽 버튼에 띄울 텍스트입니다. 미작성 시 OK 가 출력됩니다.</param>
    /// <param name="onLeftClick">왼쪽 버튼을 눌렀을 때, 실행할 액션입니다. 매개변수가 없는 함수를 호출하셔도 되고, 화살표 함수를 사용하셔도 됩니다.</param>
    /// <param name="rightText">오른쪽 버튼에 띄울 텍스트입니다. 미작성 시 Cancel 이 출력됩니다.</param>
    /// <param name="onRightClick">오른쪽 버튼을 눌렀을 때, 실행할 액션입니다. 매개변수가 없는 함수를 호출하셔도 되고, 화살표 함수를 사용하셔도 됩니다.</param>
    public void ShowOKCancelPopup(string message,
                          string leftText = "OK", Action onLeftClick = null,
                          string rightText = "Cancel", Action onRightClick = null)
    {
        if (_popupPanel == null) {
            return;
        }

        _popupPanel.SetShow(message, leftText, onLeftClick, rightText, onRightClick);
    }

    /// <summary>
    /// 확인버튼이 가운데 하나만 있는 팝업을 보여줍니다.
    /// </summary>
    /// <param name="message">팝업 가운데에 띄울 메세지 내용입니다.</param>
    /// <param name="leftText">가운데 버튼에 띄울 텍스트입니다. 미작성 시 OK 가 출력됩니다.</param>
    /// <param name="onLeftClick">가운데 버튼을 눌렀을 때, 실행할 액션입니다. 매개변수가 없는 함수를 호출하셔도 되고, 화살표 함수를 사용하셔도 됩니다.</param>
    public void ShowOKPopup(string message,
                      string leftText = "OK", Action onLeftClick = null)
    {
        if (_popupPanel == null) {
            return;
        }

        _popupPanel.SetShow(message, leftText, onLeftClick);
    }

    /// <summary>
    /// 팝업을 숨깁니다.
    /// </summary>
    public void HidePopup()
    {
        _popupPanel?.SetHide();
    }

    public async Task ShowPlayerInfo(string uid)
    {
        _infoUI.gameObject.SetActive(true);
        var info = await Database_RecordManager.Instance.LoadRankData(uid);
        _infoUI.SetInfoText(info);
    }
    
    #endregion // public funcs
}
