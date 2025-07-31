using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartPanel : UIBase
{
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private Button _gameStartButton;
    [SerializeField] private Button _signOutButton;
    [SerializeField] private Button _deleteAccountButton;

    public Action OnClickGameStart { get; set; }
    public Action OnClickSignOut { get; set; }
    public Action OnClickDeleteAccount { get; set; }

    private void Start()
    {
        _gameStartButton.onClick.AddListener(() => OnClickGameStart?.Invoke());
        // 로비 씬으로 전환
        _gameStartButton.onClick.AddListener(() => SceneManager.LoadScene("MainScene"));

        _signOutButton.onClick.AddListener(() => {
            CYH_FirebaseManager.Auth.SignOut();
            OnClickSignOut?.Invoke();
        });

        _deleteAccountButton.onClick.AddListener(() => OnClick_DelteButton());

        Debug.Log("------유저 정보------");
        Debug.Log($"유저 닉네임 : {CYH_FirebaseManager.Auth.CurrentUser.DisplayName}");
        Debug.Log($"유저 ID : {CYH_FirebaseManager.Auth.CurrentUser.UserId}");
        Debug.Log($"이메일 : {CYH_FirebaseManager.Auth.CurrentUser.Email}");
    }

    private void OnEnable()
    {
        SetNicknameField();
    }

    private void OnDisable()
    {
        _nicknameText.text = "";
    }

    /// <summary>
    /// 닉네임 text에 뜨는 닉네임을 변경하는 메세드
    /// </summary>
    public void SetNicknameField()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null && !string.IsNullOrEmpty(user.DisplayName))
        {
            _nicknameText.text = $"{user.DisplayName} 님";
        }
        else if (user.IsAnonymous)
        {
            //_nicknameText.text = $"{user.DisplayName} 님";
            _nicknameText.text = $"게스트 님";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnClick_DelteButton()
    {
        PopupManager.Instance.ShowOKCancelPopup("정말로 탈퇴하시겠습니까?\r\n모든 기록이 삭제될 수 있습니다.\r\n",
            "탈퇴", () => DeleteUser(), "취소", () => PopupManager.Instance.HidePopup());
    }

    /// <summary>
    /// 
    /// </summary>
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
                if(currentUser != null)
                {
                    CYH_FirebaseManager.Auth.SignOut();
                }
                OnClickDeleteAccount?.Invoke();
            });
    }
}
