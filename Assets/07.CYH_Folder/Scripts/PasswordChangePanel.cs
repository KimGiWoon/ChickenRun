using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PasswordChangePanel : UIBase
{
    [Header("Button")]
    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _passwordChangeButton;

    [Header("InputField")]
    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _passwordField;
    [SerializeField] private TMP_InputField _newPasswordField;

    public Action OnClickClosePopup;
    public Action OnClickPasswordChange;


    private void Start()
    {
        // 팝업 닫기 버튼
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());

        // 비밀번호 변경 버튼
        _passwordChangeButton.onClick.AddListener(OnClick_ChangePassword);
    }

    /// <summary>
    /// 안내 메세지 팝업을 띄우고 닫는 메서드
    /// </summary>
    /// <param name="message">팝업에 표시할 안내 메세지</param>
    private void ShowPopup(string message)
    {
        //Debug.LogError(message);
        PopupManager.Instance.ShowOKPopup(message, "OK", () => PopupManager.Instance.HidePopup());
    }

    /// <summary>
    /// (비밀번호)변경하기 버튼을 누르면 실행되는 메서드
    /// </summary>
    private void OnClick_ChangePassword()
    {
        CheckPassword();
    }

    #region password

    /// <summary>
    /// 새 비밀번호가 기존 비밀번호와 동일한지 체크하는 메서드
    /// </summary>
    private void CheckPassword()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        var credential = EmailAuthProvider.GetCredential(user.Email, _passwordField.text);

        user.ReauthenticateAsync(credential)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("계정 인증 취소");
                    ShowPopup("계정 인증 취소");

                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("계정 인증 실패");
                    ShowPopup("계정 인증 실패");
                    return;
                }
                if (task.IsCompleted) {
                    Debug.Log("패스워드 일치");

                    if (_emailField.text != user.Email) {
                        Debug.Log("이메일 불일치");
                        ShowPopup("계정 인증 실패");
                        return;
                    }

                    if (_passwordField.text == _newPasswordField.text) {
                        Debug.Log("새 비밀번호가 기존 비밀번호랑 같음");
                        ShowPopup("새 비밀번호가 기존 비밀번호와 동일합니다.");
                        return;
                    }

                    Debug.Log("계정 인증 성공");
                    PasswordChange();
                }
            });
    }

    /// <summary>
    /// 유저의 비밀번호를 변경하는 메서드
    /// </summary>
    private void PasswordChange()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        user.UpdatePasswordAsync(_newPasswordField.text)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled) {
                    Debug.LogError("비밀번호 변경 취소");
                    ShowPopup("비밀번호 변경 취소");
                    return;
                }

                if (task.IsFaulted) {
                    Debug.LogError("비밀번호 변경 실패");
                    ShowPopup("비밀번호 변경 실패");
                    return;
                }
                Debug.Log("비밀번호 변경 성공");

                PopupManager.Instance.ShowOKPopup("비밀번호 변경 성공\r\n다시 로그인해 주세요.", "OK", () => {
                    PopupManager.Instance.HidePopup();

                    // 비밀번호 변경 패널 비활성화
                    OnClickPasswordChange?.Invoke();
                    Debug.Log("비밀번호 변경 패널 비활성화");

                    // 강제 로그아웃
                    CYH_FirebaseManager.Auth.SignOut();
                    Debug.Log("로그아웃");

                    // 로그인 씬 전환
                    SceneManager.LoadScene("LoginScene");
                    Debug.Log("씬 전환 : MainScene -> LoginScene");
                });
            });
    }

    #endregion
}
