using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoginSceneUIManager : MonoBehaviour
{
    private enum LoginUIType
    {
        MainPanel,
        LoginPanel,
        LoginOptionPanel,
        EmailLoginPanel,
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

        //// CurrentUser 존재 o ->  GameStartPanel
        //if (CYH_FirebaseManager.Instance.IsLoggedIn())
        //{
        //    Debug.Log($"IsLoggedIn : {CYH_FirebaseManager.Instance.IsLoggedIn()}");
        //    ShowUI(LoginUIType.GameStartPanel);
        //    HideUI(LoginUIType.LoginPanel);
        //    Debug.Log("CurrentUser 있음 -> GameStartPanel");
        //}
        //// CurrentUser 존재 x -> MainPanel
        //else
        //{
        //    Debug.Log($"IsLoggedIn : {CYH_FirebaseManager.Instance.IsLoggedIn()}");
        //    ShowUI(LoginUIType.MainPanel);
        //    Debug.Log("CurrentUser 없음 -> MainPanel");
        //}

        ShowUI(LoginUIType.MainPanel);

        foreach (var ui in _uiList)
        {
            if (ui is MainPanel_LoginScene mainPanel)
            {
                mainPanel.OnClickTouch = () =>
                {
                    if (CYH_FirebaseManager.Instance.IsLoggedIn())
                    {
                        Debug.Log($"IsLoggedIn : {CYH_FirebaseManager.Instance.IsLoggedIn()}");
                        ShowUI(LoginUIType.GameStartPanel);
                        HideUI(LoginUIType.MainPanel);
                        Debug.Log("CurrentUser 있음 -> GameStartPanel");
                    }

                    else
                    {
                        Debug.Log($"IsLoggedIn : {CYH_FirebaseManager.Instance.IsLoggedIn()}");
                        ShowUI(LoginUIType.LoginPanel);
                        HideUI(LoginUIType.MainPanel);
                        Debug.Log("CurrentUser 없음 -> LoginPanel");
                    }
                };
            }

            else if (ui is LoginPanel loginPanel)
            {
                // 로그인 버튼
                loginPanel.OnClickLogin = () => ShowUI(LoginUIType.LoginOptionPanel);
                loginPanel.OnClickSignup = () => ShowUI(LoginUIType.SignUpPanel);
                //loginPanel.OnClickSocialLogin = () => ShowUI(LoginUIType.LoginOptionPanel);
            }

            else if (ui is LoginOptionPanel loginOptionPanel)
            {
                // 팝업 닫기 버튼
                loginOptionPanel.OnClickClosePopup = () => HideUI(LoginUIType.LoginOptionPanel);

                // 이메일 로그인 버튼
                loginOptionPanel.OnClickEmailLogin = () =>
                {
                    ShowUI(LoginUIType.EmailLoginPanel);
                    HideUI(LoginUIType.LoginOptionPanel);
                };
            }

            else if (ui is EmailLoginPanel emailLoginPanel)
            {
                // 닫기 버튼
                emailLoginPanel.OnClickClosePopup = () => HideUI(LoginUIType.EmailLoginPanel);

                // 이메일 인증 팝업 확인 버튼
                emailLoginPanel.OnClickEmailConfirm = () => HideUI(LoginUIType.EmailLoginPanel);

                // 이메일 로그인 성공
                emailLoginPanel.LoginCompleted = () =>
                {
                    ShowUI(LoginUIType.GameStartPanel);
                    HideUI(LoginUIType.EmailLoginPanel);
                    HideUI(LoginUIType.LoginPanel);
                };
            }

            else if (ui is SignUpPanel signUpPanel)
            {
                // 닫기 버튼
                signUpPanel.OnClickClosePopup = () => HideUI(LoginUIType.SignUpPanel);

                // 이메일 발송 팝업 확인 버튼
                signUpPanel.OnClickEmailCheck = () => HideUI(LoginUIType.SignUpPanel);
            }

            else if (ui is SocialLoginPanel socialPanel)
            {
                // 닫기 버튼
                socialPanel.OnClickClosePopup = () => HideUI(LoginUIType.LoginOptionPanel);
            }

            else if (ui is LinkPanel linkPanel)
            {
                // 닫기 버튼
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
            HideUI(LoginUIType.LoginOptionPanel);
        };

        // 게스트 로그인 성공
        _guestLogin.LoginCompleted = () =>
        {
            ShowUI(LoginUIType.GameStartPanel);
            HideUI(LoginUIType.LoginPanel);
            HideUI(LoginUIType.LoginOptionPanel);
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