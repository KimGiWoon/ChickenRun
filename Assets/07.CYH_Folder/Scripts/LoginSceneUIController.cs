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
        AccountPanel,
        GameStartPanel
    }

    [SerializeField] private List<UIBase> _uiList;
    [SerializeField] private GoogleLogin _googleLogin;
    [SerializeField] private GuestLogin _guestLogin;


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
                //loginPanel.OnClickLinkAccount = () => ShowUI(LoginUIType.LinkPanel);

                // 유저 정보 변경 테스트용
                //loginPanel.OnClickChangeAccountInfo = () => ShowUI(LoginUIType.AccountPanel);

                // 이메일 로그인 성공 시 GameStartPanel 활성화
                loginPanel.LoginCompleted = () => ShowGameStartUI();
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
                // 회원탈퇴 버튼
                linkPanel.OnClickSignOut = () =>
                {
                    HideUI(LoginUIType.GameStartPanel);
                    ShowUI(LoginUIType.Login);
                };
            }

            else if (ui is AccountPanel accountPanel)
            {
                accountPanel.OnClickClosePopup = () => HideUI(LoginUIType.AccountPanel);
            }

            else if (ui is GameStartPanel gameStartPanel)
            {
                // 게임 시작 버튼
                gameStartPanel.OnClickGameStart = () => HideUI(LoginUIType.GameStartPanel);

                // 로그아웃 버튼
                gameStartPanel.OnClickSignOut = () =>
                {
                    HideUI(LoginUIType.GameStartPanel);
                    ShowUI(LoginUIType.Login);
                    PopupManager.Instance.ShowOKPopup("로그아웃 성공", "OK", () => PopupManager.Instance.HidePopup());
                };

                // 회원탈퇴 버튼
                gameStartPanel.OnClickDeleteAccount = () =>
                {
                    HideUI(LoginUIType.GameStartPanel);
                    ShowUI(LoginUIType.Login);
                    PopupManager.Instance.ShowOKPopup("회원 탈퇴 성공", "OK", () => PopupManager.Instance.HidePopup());
                };
            }
        }

        // 구글 로그인 성공
        _googleLogin.LoginCompleted = () =>
        {
            ShowUI(LoginUIType.GameStartPanel);
            HideUI(LoginUIType.Login);
            HideUI(LoginUIType.SocialPanel);
        };

        // 게스트 로그인 성공
        _guestLogin.LoginCompleted = () =>
        {
            Debug.Log("게스트 로그인 성공 후 ShowUI");
            ShowUI(LoginUIType.GameStartPanel);
            HideUI(LoginUIType.Login);
            HideUI(LoginUIType.SocialPanel);
        };
    }

    /// <summary>
    /// 패널을 여는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void ShowUI(LoginUIType type)
    {
        _uiList[(int)type].SetShow();
    }

    /// <summary>
    /// 패널을 숨기는 메서드
    /// </summary>
    /// <param name="type"></param>
    private void HideUI(LoginUIType type)
    {
        _uiList[(int)type].SetHide();
    }

    /// <summary>
    /// GameStart 패널을 여는 메서드
    /// Login 패널 숨김
    /// </summary>
    private void ShowGameStartUI()
    {
        ShowUI(LoginUIType.GameStartPanel);
        HideUI(LoginUIType.Login);
    }
}