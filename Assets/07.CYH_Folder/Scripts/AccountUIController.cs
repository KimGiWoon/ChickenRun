using System.Collections.Generic;
using UnityEngine;

public class AccountUIController : MonoBehaviour
{
    private enum AccountUIType
    {
        AccountPanel,
        NicknameChangePanel,
        PasswordChangePanel
    }

    [SerializeField] private List<UIBase> _uiList;


    private void Start()
    {
        foreach (var ui in _uiList)
        {
            if (ui is AccountPanel accountPanel)
            {
                // 팝업 닫기 버튼
                accountPanel.OnClickClosePopup = () => HideUI(AccountUIType.AccountPanel);

                // 닉네임 변경 버튼
                accountPanel.OnClickNicknameChange = () =>
                {
                    ShowUI(AccountUIType.NicknameChangePanel);
                    HideUI(AccountUIType.AccountPanel);
                };

                // 비밀번호 변경 버튼
                accountPanel.OnClickPasswordChange = () =>
                {
                    ShowUI(AccountUIType.PasswordChangePanel);
                    HideUI(AccountUIType.AccountPanel);
                };

                // 회원탈퇴 버튼
                accountPanel.OnClickDeleteAccount = () =>
                {
                    HideUI(AccountUIType.AccountPanel);
                };

                // 로그아웃 버튼
                accountPanel.OnClickSignOut = () =>
                {
                    HideUI(AccountUIType.AccountPanel);
                };
            }
            else if (ui is NicknameChangePanel nicknameChangePanel)
            {
                // 팝업 닫기 버튼
                nicknameChangePanel.OnClickClosePopup = () => HideUI(AccountUIType.NicknameChangePanel);

                // 닉네임 변경 버튼
                nicknameChangePanel.OnClickNicknameChange =() => HideUI(AccountUIType.NicknameChangePanel);
            }
            else if (ui is PasswordChangePanel passwordChangePanel)
            {
                // 팝업 닫기 버튼
                passwordChangePanel.OnClickClosePopup = () => HideUI(AccountUIType.PasswordChangePanel);
               
                // 비밀번호 변경 버튼
                passwordChangePanel.OnClickPasswordChange = () => HideUI(AccountUIType.PasswordChangePanel);
            }
        }
    }

    /// <summary>
    /// 패널을 여는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void ShowUI(AccountUIType type)
    {
        _uiList[(int)type].SetShow();
    }

    /// <summary>
    /// 패널을 숨기는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void HideUI(AccountUIType type)
    {
        _uiList[(int)type].SetHide();
    }
}
