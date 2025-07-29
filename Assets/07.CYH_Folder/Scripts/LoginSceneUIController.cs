using System.Collections.Generic;
using UnityEngine;

public class LoginSceneUIManager : MonoBehaviour
{
    private enum LoginUIType
    {
        Login,
        SocialPanel,
        SignUpPanel,
        LinkPanel,
        AccountPanel
    }

    [SerializeField] private List<UIBase> _uiList;


    private void Start()
    {
        ShowUI(LoginUIType.Login);

        foreach (var ui in _uiList)
        {
            if (ui is LoginPanel loginPanel)
            {
                loginPanel.OnClickSignup = () => ShowUI(LoginUIType.SignUpPanel);
                loginPanel.OnClickSocialLogin = () => ShowUI(LoginUIType.SocialPanel);

                // 계정 전환 테스트용
                loginPanel.OnClickLinkAccount = () => ShowUI(LoginUIType.LinkPanel);

                // 유저 정보 변경 테스트용
                loginPanel.OnClickChangeAccountInfo = () => ShowUI(LoginUIType.AccountPanel);
            }

            else if (ui is SignUpPanel signUpPanel)
            {
                signUpPanel.OnClickClosePopup = () => HideUI(LoginUIType.SignUpPanel);
            }

            else if (ui is SocialLoginPanel socialPanel)
            {
                socialPanel.OnClickClosePopup = () => HideUI(LoginUIType.SocialPanel);
            }

            else if (ui is LinkPanel linkPanel)
            {
                linkPanel.OnClickClosePopup = () => HideUI(LoginUIType.LinkPanel);
            }

            else if (ui is AccountPanel accountPanel)
            {
                accountPanel.OnClickClosePopup = () => HideUI(LoginUIType.AccountPanel);
            }
        }
    }

    /// <summary>
    /// 팝업을 여는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void ShowUI(LoginUIType type)
    {
        _uiList[(int)type].SetShow();
    }

    /// <summary>
    /// 팝업을 숨기는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void HideUI(LoginUIType type)
    {
        _uiList[(int)type].SetHide();
    }
}