using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginSceneUIManager : MonoBehaviour
{
    private enum LoginUIType
    {
        LoginPanel,
        SocialPanel,
        SignUpPanel,
        LinkPanel,
        AccountPanel,
        GameStartPanel
    }

    [SerializeField] private List<UIBase> _uiList;
    [SerializeField] private GoogleLogin _googleLogin;
    [SerializeField] private GuestLogin _guestLogin;


    private IEnumerator Start()
    {
        // FirebaseManager Init 끝난 후 실행
        while (!CYH_FirebaseManager.Instance.IsFirebaseReady)
        {
            yield return null;
        }

        // CurrentUser 존재 o ->  GameStartPanel
        if (CYH_FirebaseManager.Instance.IsLoggedIn())
        {
            Debug.Log($"IsLoggedIn : {CYH_FirebaseManager.Instance.IsLoggedIn()}");
            ShowUI(LoginUIType.GameStartPanel);
            HideUI(LoginUIType.LoginPanel);
            Debug.Log("CurrentUser 있음 -> GameStartPanel");
        }
        // CurrentUser 존재 x -> Login
        else
        {
            Debug.Log($"IsLoggedIn : {CYH_FirebaseManager.Instance.IsLoggedIn()}");
            ShowUI(LoginUIType.LoginPanel);
            Debug.Log("CurrentUser 없음 -> LoginPanel");
        }

        //ShowUI(LoginUIType.Login);

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
                    ShowUI(LoginUIType.LoginPanel);
                };
            }

            else if (ui is GameStartPanel gameStartPanel)
            {
                // 게임 시작 버튼
                gameStartPanel.OnClickGameStart = () => HideUI(LoginUIType.GameStartPanel);

                // 로그아웃 버튼
                gameStartPanel.OnClickSignOut = () =>
                {
                    HideUI(LoginUIType.GameStartPanel);
                    ShowUI(LoginUIType.LoginPanel);
                    PopupManager.Instance.ShowOKPopup("로그아웃 성공", "OK", () => PopupManager.Instance.HidePopup());
                };

                // 회원탈퇴 버튼
                gameStartPanel.OnClickDeleteAccount = () =>
                {
                    HideUI(LoginUIType.GameStartPanel);
                    ShowUI(LoginUIType.LoginPanel);
                    PopupManager.Instance.ShowOKPopup("회원 탈퇴 성공", "OK", () => PopupManager.Instance.HidePopup());
                };
            }
        }

        // 구글 로그인 성공
        _googleLogin.LoginCompleted = () =>
        {
            ShowUI(LoginUIType.GameStartPanel);
            HideUI(LoginUIType.LoginPanel);
            HideUI(LoginUIType.SocialPanel);
        };

        // 게스트 로그인 성공
        _guestLogin.LoginCompleted = () =>
        {
            Debug.Log("게스트 로그인 성공 후 ShowUI");
            ShowUI(LoginUIType.GameStartPanel);
            HideUI(LoginUIType.LoginPanel);
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
        HideUI(LoginUIType.LoginPanel);
    }
}