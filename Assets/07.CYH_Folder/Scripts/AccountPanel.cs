using Firebase.Auth;
using Firebase.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AccountPanel : UIBase
{
    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _nicknameChaneButton;
    [SerializeField] private Button _passwordChangeButton;
    [SerializeField] private Button _deleteAccountButton;
    [SerializeField] private Button _signoutButton;

    public Action OnClickClosePopup;
    public Action OnClickNicknameChange;
    public Action OnClickPasswordChange;
    public Action OnClickDeleteAccount;
    public Action OnClickSignOut;

    private void Start()
    {
        // 팝업 닫기 버튼
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());

        // 닉네임 변경 버튼
        _nicknameChaneButton.onClick.AddListener(() => OnClickNicknameChange?.Invoke());
        
        // 패스워드 변경 버튼
        _passwordChangeButton.onClick.AddListener(() => OnClickPasswordChange?.Invoke());

        // 회원탈퇴 버튼
        _deleteAccountButton.onClick.AddListener(() =>
        {
            OnClick_DelteButton();
        });

        // 로그아웃 버튼
        _signoutButton.onClick.AddListener(() =>
        {
            CYH_FirebaseManager.Auth.SignOut();
            OnClickSignOut?.Invoke();
            SceneManager.LoadScene("LoginScene");
        });
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

    #region deleteAccount

    private void OnClick_DelteButton()
    {
        PopupManager.Instance.ShowOKCancelPopup("정말로 탈퇴하시겠습니까?\r\n모든 기록이 삭제될 수 있습니다.\r\n",
            "탈퇴", () => DeleteUser(), 
            "취소", () => 
            { 
                PopupManager.Instance.HidePopup();
                SceneManager.LoadScene("LoginScene");
            });
    }

    private void DeleteUser()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;

        currentUser.DeleteAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("유저 삭제 취소");
                }
                if (task.IsCanceled)
                {
                    Debug.LogError("유저 삭제 실패");
                }

                currentUser.DeleteAsync();
                Debug.Log("유저 삭제 성공");

                // 현재 로그인된 유저가 있으면 로그아웃
                if (currentUser != null)
                {
                    CYH_FirebaseManager.Auth.SignOut();
                }
                // LoginScene으로 씬 전환
                OnClickDeleteAccount?.Invoke();
            });
    }

    #endregion

}
