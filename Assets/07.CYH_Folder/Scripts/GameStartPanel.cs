using Firebase.Auth;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;
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
    public Action<string> OnSetNicknameField { get; set; }

    private void Start()
    {
        _gameStartButton.onClick.AddListener(() => OnClickGameStart?.Invoke());

        // 로비 씬으로 전환
        _gameStartButton.onClick.AddListener(() =>
        {
            LoadingManager.Instance.LoadSceneWithLoading("LoadingScene", "MainScene", 2f);

            // 포톤 초기화
            CYH_FirebaseManager.Instance.OnFirebaseLoginSuccess();
        });

        _signOutButton.onClick.AddListener(() =>
        {
            CYH_FirebaseManager.Auth.SignOut();
            OnClickSignOut?.Invoke();
        });

        _deleteAccountButton.onClick.AddListener(() => OnClick_DelteButton());

        OnSetNicknameField += SetNicknameField_google;

        Debug.Log("------유저 정보(GameStartScene)------");
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
    /// 닉네임 text에 표시되는 닉네임을 변경하는 메세드
    /// </summary>
    public void SetNicknameField()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        Debug.Log("GameStartPanel nicknamefield");

        if (user != null)
        {
            _nicknameText.text = $"{user.DisplayName}";

            if (user.IsAnonymous)
            {
                _nicknameText.text = $"게스트 님";
                Debug.Log("nicknamefield : 게스트");
            }
            else if (user.ProviderId == "google.com")
            {
                SetNicknameField_google(user.DisplayName);
                //_nicknameText.text = $"게스트 님";
                Debug.Log("nicknamefield : 구글");
            }
            else if(user.ProviderId == "password")
            {
                _nicknameText.text = $"{user.DisplayName} 님";
                Debug.Log("nicknamefield : 이메일");
            }
            else if(string.IsNullOrEmpty(user.DisplayName))
            {
                _nicknameText.text = $"닭1 님";
            }
        }
        else
        {
            Debug.LogError("user == null");
        }
    }

    /// <summary>
    /// 닉네임 text에 표시되는 닉네임을 구글 계정 닉네임으로 변경하는 메세드
    /// </summary>
    /// <param name="googleDisplayName">구글 계정 Displayname</param>
    public void SetNicknameField_google(string googleDisplayName)
    {
        _nicknameText.text = $"{googleDisplayName} 님";
    }

    /// <summary>
    /// 회원탈퇴 버튼 클릭 시 호출되는 메서드
    /// </summary>
    private void OnClick_DelteButton()
    {
        PopupManager.Instance.ShowOKCancelPopup("정말로 탈퇴하시겠습니까?\r\n모든 기록이 삭제될 수 있습니다.\r\n",
            "탈퇴", () =>
            {
                DeleteUser();
            },
            "취소", () => PopupManager.Instance.HidePopup());
    }

    /// <summary>
    /// 현재 로그인된 계정을 FirebaseAuth / Firebase DB에서 삭제하는 메서드
    /// </summary>
    private void DeleteUser()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;
        Debug.Log($" (Auth Delete_1) 현재 로그인된 유저 uid : {currentUser.UserId}");

        // DB 데이터 삭제
        Utility.DeleteUserUID();

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

                Debug.Log("유저 삭제 성공");
                Debug.Log($" (Auth Delete_2) 현재 로그인된 유저 uid : {currentUser.UserId}");

                // 현재 로그인된 유저가 있으면 로그아웃
                if (currentUser != null)
                {
                    CYH_FirebaseManager.Auth.SignOut();
                }
                OnClickDeleteAccount?.Invoke();
            });
    }
}
