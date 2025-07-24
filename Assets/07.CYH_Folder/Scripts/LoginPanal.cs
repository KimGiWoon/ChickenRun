using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이메일/비밀번호 입력을 통한 로그인 기능을 담당하는 UI 패널 클래스
/// </summary>
public class LoginPanel : UIBase
{
    [SerializeField] private GameObject _signupPanel;
    [SerializeField] private GameObject _socialLoginPanel;

    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _passwordField;

    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _socialLoginButton;
    [SerializeField] private Button _signupButton;

    private void Start()
    {
        _loginButton.onClick.AddListener(OnClick_Login);
        _socialLoginButton.onClick.AddListener(() => SetShow(_socialLoginPanel));
        _signupButton.onClick.AddListener(() => SetShow(_signupPanel));
    }

    public void SetShow(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public override void RefreshUI()
    {
        _emailField.text = "";
        _passwordField.text = "";
    }

    /// <summary>
    /// 로그인 버튼 클릭 시 호출되는 메서드
    /// 입력한 이메일/비밀번호로 Firebase 인증 시도
    /// 로그인 성공: 유저 정보를 출력하고 로그인 패널 비활성화
    /// 로그인 실패: 에러 코드 출력
    /// </summary>
    private void OnClick_Login()
    {
        CYH_FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(_emailField.text, _passwordField.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("로그인 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError($"로그인 실패 / 원인: {task.Exception}");
                    return;
                }

                if (task.IsCompletedSuccessfully)
                {
                    AuthResult result = task.Result;
                    FirebaseUser user = result.User;

                    Debug.Log("로그인 성공");
                    Debug.Log("------유저 정보------");
                    Debug.Log($"유저 이름 : {user.DisplayName}");
                    Debug.Log($"유저 ID: {user.UserId}");
                    Debug.Log($"이메일 : {user.Email}");

                    RefreshUI();
                }
            });
    }
}
