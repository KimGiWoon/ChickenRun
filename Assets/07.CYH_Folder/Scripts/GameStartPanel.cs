using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System;
using System.Linq;
using System.Threading.Tasks;
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
    public Action IsUserOnline { get; set; }                // 중복 접속 시 호출되는 이벤트
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

            // BGM 멈춤
            SoundManager.Instance.StopBGM();
        });

        // 로그아웃 버튼 클릭
        _signOutButton.onClick.AddListener(() =>
        {
            // Online -> Offline
            Utility.SetOffline();
            
            // 익명 계정 -> 로그아웃 시 계정 삭제
            if (CYH_FirebaseManager.Auth.CurrentUser.IsAnonymous)
            {
                DeleteUser();
            }

            // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
            bool isGoogleUser = CYH_FirebaseManager.Auth.CurrentUser.ProviderData.Any(provider => provider.ProviderId == "google.com");
            if (isGoogleUser)
            {
                GoogleSignIn.DefaultInstance.SignOut();
                GoogleSignIn.DefaultInstance.Disconnect();
            }

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

    private async void OnEnable()
    {
        SetNicknameField();

        // 유저 온라인 체크
        await CheckIsOnline();
    }

    private void OnDisable()
    {
        _nicknameText.text = "";
    }


    /// <summary>
    /// 로그인한 유저의 로그인 상태 IsOnline = true 로 전환하는 메서드
    /// </summary>
    private async Task CheckIsOnline()
    {
        await IsOnline();
    }

    /// <summary>
    /// 닉네임 text에 표시되는 닉네임을 변경하는 메세드
    /// </summary>
    public void SetNicknameField()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        Debug.Log("GameStartPanel nicknamefield");
        user.ReloadAsync();


        if (user != null)
        {
            _nicknameText.text = $"{user.DisplayName} 님";

            if (user.IsAnonymous)
            {
                _nicknameText.text = $"{user.DisplayName} 님";
                Debug.Log("nicknamefield : 게스트");
            }
            else if (user.ProviderId == "google.com")
            {
                SetNicknameField_google(user.DisplayName);
                Debug.Log("nicknamefield : 구글");
            }
            else if (user.ProviderId == "password")
            {
                _nicknameText.text = $"{user.DisplayName} 님";
                Debug.Log("nicknamefield : 이메일");
            }
            else if (string.IsNullOrEmpty(user.DisplayName))
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
                if (task.IsFaulted)
                {
                    Debug.LogError("유저 삭제 실패");
                }

                Debug.Log("유저 삭제 성공");
                Debug.Log($" (Auth Delete_2) 현재 로그인된 유저 uid : {currentUser.UserId}");

                // 현재 로그인된 유저가 있으면 로그아웃
                if (currentUser != null)
                {
                    // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                    bool isGoogleUser = currentUser.ProviderData.Any(provider => provider.ProviderId == "google.com");
                    if (isGoogleUser)
                    {
                        GoogleSignIn.DefaultInstance.SignOut();
                        GoogleSignIn.DefaultInstance.Disconnect();
                    }
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
                    CYH_FirebaseManager.Auth.SignOut();
                }
                OnClickDeleteAccount?.Invoke();
            });
    }

    /// <summary>
    /// 계정 동시 접속을 제한하는 메서드
    /// 현재 유저의 온라인 상태를 확인하고 이미 접속 중이라면 로그인 제한
    /// </summary>
    private async Task IsOnline()
    {
        FirebaseAuth auth = CYH_FirebaseManager.Auth;
        string uid = CYH_FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef = CYH_FirebaseManager.DataReference.Child("UserData").Child(uid).Child("IsOnline");

        Debug.Log("IsOnline 호출 완료");

        // 현재 접속 상태 확인
        bool isOnline = false;
        DataSnapshot snapshot = await userRef.GetValueAsync();
        if (snapshot.Exists && snapshot.Value is bool boolVal)
        {
            isOnline = boolVal;
        }

        Debug.Log(isOnline);

        // 동시 접속 제한
        if (isOnline)
        {
            Debug.LogError("IsOnline: 이미 로그인된 계정");
            PopupManager.Instance.ShowOKPopup("이미 로그인된 계정입니다.", "OK", () =>
            {
                PopupManager.Instance.HidePopup();

                //로그아웃 처리 및 mainPanel 전환
                Utility.SetOffline();

                // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                bool isGoogleUser = CYH_FirebaseManager.Auth.CurrentUser.ProviderData.Any(provider => provider.ProviderId == "google.com");
                if (isGoogleUser)
                {
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
                }

                auth.SignOut();
                IsUserOnline?.Invoke();
            });
        }

        // 로그인 -> IsOnline = true로 설정
        // disconnect -> 자동 IsOnline = false
        await userRef.SetValueAsync(true);
        Debug.Log($"로그인 / 유저 UID : {uid} IsOnline: {snapshot.Value}");
        
        await userRef.OnDisconnect().SetValue(false);
       
        Application.quitting += () => userRef.SetValueAsync(false);
    }
}
