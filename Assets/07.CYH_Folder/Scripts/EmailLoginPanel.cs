using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이메일/비밀번호 입력을 통한 로그인 기능을 담당하는 UI 패널 클래스
/// </summary>
public class EmailLoginPanel : UIBase
{
    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _passwordField;

    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _confirmButton;

    public Action OnClickEmailConfirm { get; set; }
    public Action OnClickClosePopup { get; set; }
    public Action LoginCompleted { get; set; }

    private void Start()
    {
        _confirmButton.onClick.AddListener(OnClick_Login);
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
    }

    private void OnDisable()
    {
        RefreshUI();
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
    /// 로그인 실패: 로그인 실패 안내 팝업 활성화
    /// </summary>
    public void OnClick_Login()
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

                    // 팝업 (로그인 실패)
                    PopupManager.Instance.ShowOKPopup("로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                    return;
                }

                if (task.IsCompletedSuccessfully)
                {
                    AuthResult result = task.Result;
                    FirebaseUser user = result.User;

                    //PopupManager.Instance.ShowOKPopup("로그인 성공", "OK", () => PopupManager.Instance.HidePopup());

                    Debug.Log("------유저 정보------");
                    Debug.Log($"유저 이름 : {user.DisplayName}");
                    Debug.Log($"유저 ID: {user.UserId}");
                    Debug.Log($"이메일 : {user.Email}");

                    if (!user.IsEmailVerified)
                    {
                        // 팝업 (이메일 인증 요청)
                        PopupManager.Instance.ShowOKPopup("이메일 인증을 완료해주세요.", "OK", () =>
                        {
                            PopupManager.Instance.HidePopup();
                            //OnClickEmailConfirm?.Invoke();
                        });

                        // 강제 로그아웃
                        CYH_FirebaseManager.Auth.SignOut();
                        Utility.SetOffline();
                        Debug.Log("로그아웃");
                        return;
                    }

                    // EmailLoginPanel -> GameStartPanel 로 변경
                    if (user != null)
                    {
                        Debug.Log("이메일 로그인 성공. GameStart패널 활성화");
                        LoginCompleted?.Invoke();
                    }
                }
            });
    }
}