using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 이메일/비밀번호 입력을 통한 로그인 기능을 담당하는 UI 패널 클래스
/// </summary>
public class LoginPanel : UIBase
{
    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _passwordField;

    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _socialLoginButton;
    [SerializeField] private Button _signupButton;

    // 계정 전환 테스트용
    //[SerializeField] private Button _linkAccountButton;
    // 유저 정보 변경 테스트용
    //[SerializeField] private Button _changeAccountInfoButton;

    public Action OnClickSignup { get; set; }
    public Action OnClickSocialLogin { get; set; }

    // 계정 전환 테스트용
    //public Action OnClickLinkAccount { get; set; }

    // 유저 정보 변경 테스트용
    //public Action OnClickChangeAccountInfo { get; set; }

    public Action LoginCompleted { get; set; }

    // 로그아웃 테스트용
    //[SerializeField] private Button _signOutButton;


    private void Start()
    {
        _loginButton.onClick.AddListener(OnClick_Login);
        _socialLoginButton.onClick.AddListener(() => OnClickSocialLogin?.Invoke());
        _signupButton.onClick.AddListener(() => OnClickSignup?.Invoke());

        // 계정 전환 테스트용
        //_linkAccountButton.onClick.AddListener(() => OnClickLinkAccount?.Invoke());

        // 유저 정보 변경 테스트용
        //_changeAccountInfoButton.onClick.AddListener(() => OnClickChangeAccountInfo?.Invoke());

        // 로그아웃 테스트용
        //_signOutButton.onClick.AddListener(() => CYH_FirebaseManager.Auth.SignOut());
    }

    /// <summary>
    /// 로그인 버튼 클릭 시 호출되는 메서드
    /// 입력한 이메일/비밀번호로 Firebase 인증 시도
    /// 로그인 성공: 유저 정보를 출력하고 로그인 패널 비활성화
    /// 로그인 실패: 로그인 실패 안내 팝업 활성화
    /// </summary>
    public void OnClick_Login()
    {
        CYH_FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(_emailField.text, _passwordField.text)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("로그인 취소");
                    return;
                }

                if (task.IsFaulted) {
                    Debug.LogError($"로그인 실패 / 원인: {task.Exception}");

                    // 팝업 (로그인 실패)
                    PopupManager.Instance.ShowOKPopup("로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                    return;
                }

                if (task.IsCompletedSuccessfully) {
                    AuthResult result = task.Result;
                    FirebaseUser user = result.User;

                    //PopupManager.Instance.ShowOKPopup("로그인 성공", "OK", () => PopupManager.Instance.HidePopup());

                    Debug.Log("------유저 정보------");
                    Debug.Log($"유저 이름 : {user.DisplayName}");
                    Debug.Log($"유저 ID: {user.UserId}");
                    Debug.Log($"이메일 : {user.Email}");

                    if (!user.IsEmailVerified) {
                        // 팝업 (이메일 인증 요청)
                        PopupManager.Instance.ShowOKPopup("이메일 인증을 완료해주세요.", "OK", () => PopupManager.Instance.HidePopup());
                        CYH_FirebaseManager.Auth.SignOut();
                        Debug.Log("로그아웃");
                        return;
                    }

                    CYH_FirebaseManager.Instance.OnFirebaseLoginSuccess();

                    // LoginPanel -> GameStartPanel 로 변경
                    if (user != null)
                    {
                        Debug.Log("이메일 로그인 성공. GameStart패널 활성화");
                        LoginCompleted?.Invoke();
                    }

                    //SceneManager.LoadScene("MainScene");
                }
            });
    }
}
