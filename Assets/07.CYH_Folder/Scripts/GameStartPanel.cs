using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections;
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
    public Action<string> OnSetNicknameField { get; set; }

    private void Start()
    {
        _gameStartButton.onClick.AddListener(() => OnClickGameStart?.Invoke());
       
        // 로비 씬으로 전환
        _gameStartButton.onClick.AddListener(() =>
        {
            // TODO : <최연호> Load Test

            LoadingManager.Instance.LoadSceneWithLoading("LoadingScene", "MainScene", 2f);
            //LoadingManager.Instance.LoadSceneAsync("MainScene");

            // 포톤 초기화
            CYH_FirebaseManager.Instance.OnFirebaseLoginSuccess();
        });

        _signOutButton.onClick.AddListener(() =>
        {
            CYH_FirebaseManager.Auth.SignOut();
            OnClickSignOut?.Invoke();
        });

        _deleteAccountButton.onClick.AddListener(() => OnClick_DelteButton());

        //OnSetNicknameField += SetNicknameField;

        Debug.Log("------유저 정보------");
        Debug.Log($"유저 닉네임 : {CYH_FirebaseManager.Auth.CurrentUser.DisplayName}");
        Debug.Log($"유저 ID : {CYH_FirebaseManager.Auth.CurrentUser.UserId}");
        Debug.Log($"이메일 : {CYH_FirebaseManager.Auth.CurrentUser.Email}");

        NicknameField();
    }

    private void OnEnable()
    {
        NicknameField();
    }

    private void NicknameField()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        Debug.Log("GameStartPanel 이벤트 호출");

        if (user != null)
        {
            SetNicknameField();
        }
        else
        {
            Debug.LogError("user == null");
        }
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
        //StartCoroutine(SetNicknameFieldCoroutine());

        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        //Debug.Log("GameStartPanel 이벤트 호출");

        _nicknameText.text = $"{user.DisplayName} 님";
        Debug.Log($"user.DisplayName : {user.DisplayName}");

        //if (user != null)
        //{
        //    if(user.IsAnonymous)
        //    {
        //        Debug.Log("현재 유저 상태: 게스트");
        //        _nicknameText.text = $"{user.DisplayName} 님";
        //        Debug.Log($"user.DisplayName : {user.DisplayName}");
        //    }
        //    else if(user.ProviderId == "google.com")
        //    {
        //        foreach (var provider in user.ProviderData)
        //        {
        //            if (provider.ProviderId == "google.com")
        //            {
                       
        //                break;
        //            }
        //        }

        //        Debug.Log("현재 유저 상태: ");
        //        _nicknameText.text = $"{Utility.LoadNickname()} 님";
        //        Debug.Log($"Utility.LoadNickname() : {Utility.LoadNickname()}");
        //    }
        //}
        //else
        //{
        //    _nicknameText.text = $" 님";
        //    Debug.LogError("현재 유저 상태: null");
        //}

        //if (user != null && !string.IsNullOrEmpty(user.DisplayName))
        //{
        //    Debug.Log("현재 유저 상태: 닉네임 null 아님, 유저 null 아님");
        //    _nicknameText.text = $"{user.DisplayName} 님";
        //}
        //else if (user.IsAnonymous)
        //{
        //    Debug.LogError($"현재 유저 상태: 게스트 / Displayname : {user.DisplayName}");
        //    //_nicknameText.text = $"{user.DisplayName} 님";
        //    _nicknameText.text = $"게스트 님";
        //}
    }

    /// <summary>
    /// 닉네임 text에 표시되는 닉네임을 변경하는 메세드
    /// </summary>
    /// <param name="googleDisplayName">구글 계정 Displayname</param>
    public void SetNicknameField_google()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        
        _nicknameText.text = $"{Utility.LoadNickname()} 님";

        //if (user != null)
        //{
        //    Debug.Log("현재 유저 상태: 닉네임 null 아님, 유저 null 아님");
        //    _nicknameText.text = $"{googleDisplayName} 님";
        //}

        //else if (user.IsAnonymous)
        //{
        //    Debug.LogError($"현재 유저 상태: 게스트 / Displayname : {user.DisplayName}");
        //    _nicknameText.text = $"게스트 님";
        //}

        //else
        //{
        //    Debug.LogError($"현재 유저 상태: else / Displayname : {user.DisplayName}");
        //    _nicknameText.text = $"else 님";
        //}
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnClick_DelteButton()
    {
        PopupManager.Instance.ShowOKCancelPopup("정말로 탈퇴하시겠습니까?\r\n모든 기록이 삭제될 수 있습니다.\r\n",
            "탈퇴", () =>
            { 
                DeleteUser();
                // DB에서 UID 삭제
                //Utility.DeleteUserUID();
            }, 
            "취소", () => PopupManager.Instance.HidePopup());
    }

    /// <summary>
    /// 
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
