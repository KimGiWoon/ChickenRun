using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AccountPanel : UIBase
{
    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _nicknameChaneButton;
    [SerializeField] private Button _passwordChangeButton;
    [SerializeField] private Button _deleteAccountButton;
    [SerializeField] private Button _signoutButton;
    [SerializeField] private Button _googleLinkButton;

    [SerializeField] private TMP_Text _nicknameText;

    public Action OnClickClosePopup;
    public Action OnClickNicknameChange;
    public Action OnClickPasswordChange;
    public Action OnClickDeleteAccount;
    public Action OnClickSignOut;

    private void Start()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;

        // 팝업 닫기 버튼
        _closePopupButton.onClick.AddListener(() =>
        {
            OnClickClosePopup?.Invoke();
            Debug.Log("닫기 버튼 클릭");
        });

        // 닉네임 변경 버튼
        _nicknameChaneButton.onClick.AddListener(() =>
        {
            // 게스트 변경 제한
            if (user.IsAnonymous)
            {
                PopupManager.Instance.ShowOKPopup("게스트 계정은 닉네임을 변경할 수 없습니다.", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }
            else
            {
                OnClickNicknameChange?.Invoke();
            }
        });

        // 패스워드 변경 버튼
        _passwordChangeButton.onClick.AddListener(() => {
            if(!(user.ProviderId == "password"))
            {
                PopupManager.Instance.ShowOKPopup("패스워드를 설정할 수 없는 계정입니다.", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }
            OnClickPasswordChange?.Invoke();
        });

        // 회원탈퇴 버튼
        _deleteAccountButton.onClick.AddListener(() =>
        {
            OnClick_DelteButton();
        });

        // 로그아웃 버튼
        _signoutButton.onClick.AddListener(() =>
        {
            PopupManager.Instance.ShowOKCancelPopup("로그아웃하시겠습니까?",
                "네", () =>
                {

                    Utility.SetOffline();
                    
                    // 익명 계정 -> 로그아웃 시 계정 삭제
                    if (user.IsAnonymous)
                    {
                        DeleteUser();
                    }

                    // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                    bool isGoogleUser = user.ProviderData.Any(provider => provider.ProviderId == "google.com");
                    if (isGoogleUser)
                    {
                        Debug.Log("구글 계정 로그아웃 처리 및 계정과 앱 연결 해제 : isGoogleUser ");
                        GoogleSignIn.DefaultInstance.SignOut();
                        GoogleSignIn.DefaultInstance.Disconnect();
                    }

                    Debug.Log("구글 계정 로그아웃 처리 및 계정과 앱 연결 해제");
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();

                    CYH_FirebaseManager.Auth.SignOut();
                    OnClickSignOut?.Invoke();
                    SceneManager.LoadScene("LoginScene");
                },
                "아니요", null);
        });

        // 구글 계정 연동 버튼 클릭
        _googleLinkButton.onClick.AddListener(OnClick_LinkWithGoogle);
    }


    /// <summary>
    /// 익명 계정을 구글 가입 계정으로 전환
    /// </summary>
    public void OnClick_LinkWithGoogle()
    {
        // 계정 전환 가능 여부 체크
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;

        if (user == null || !user.IsAnonymous)
        {
            PopupManager.Instance.ShowOKPopup("게스트x. 계정 전환 불가", "OK", () => PopupManager.Instance.HidePopup());
            return;
        }

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"구글 로그인 실패 / 원인: {task.Exception}");

                PopupManager.Instance.ShowOKPopup("구글 로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }

            GoogleSignInUser googleUser = task.Result;
            string idToken = googleUser.IdToken;

            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

            CYH_FirebaseManager.Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(async linkTask =>
            {
                if (linkTask.IsCanceled)
                {
                    PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 취소", "OK", () => PopupManager.Instance.HidePopup());
                    return;
                }

                if (linkTask.IsFaulted)
                {
                    PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 실패", "OK", () => PopupManager.Instance.HidePopup());
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
                    return;
                }

                Firebase.Auth.AuthResult linkedUser = linkTask.Result;

                string googleDisplayName = googleUser.DisplayName;
                Debug.Log($"구글 계정 닉네임 : {googleDisplayName}");

                if (CYH_FirebaseManager.Auth.CurrentUser == null)
                {
                    Debug.Log("현재 유저 상태: CurrentUser is null");
                }
                else
                {
                    Debug.Log("현재 유저 상태: CurrentUser");
                }

                // 구글 닉네임 변경 
                await Utility.SetGoogleNickname(user, googleDisplayName);
                await user.ReloadAsync();


                Debug.Log("------유저 정보------");
                Debug.Log($"유저 이름 : {user.DisplayName}");
                Debug.Log($"유저 ID: {user.UserId}");
                Debug.Log($"이메일 : {user.Email}");

                PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 성공\r\n 다시 로그인 해주세요.", "OK", () =>
                {
                    PopupManager.Instance.HidePopup();

                    // SignOut
                    Utility.SetOffline();

                    // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                    bool isGoogleUser = user.ProviderData.Any(provider => provider.ProviderId == "google.com");
                    if (isGoogleUser)
                    {
                        GoogleSignIn.DefaultInstance.SignOut();
                        GoogleSignIn.DefaultInstance.Disconnect();
                    }

                    CYH_FirebaseManager.Auth.SignOut();

                    // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();

                    // 팝업 비활성화
                    OnClickClosePopup?.Invoke();

                    // 로그인 씬으로 전환
                    SceneManager.LoadScene("LoginScene");
                });
            });
        });
    }

    private void OnEnable()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
        _nicknameText.text = $"{user.DisplayName}";
    }

    #region deleteAccount

    private void OnClick_DelteButton()
    {
        PopupManager.Instance.ShowOKCancelPopup("정말로 탈퇴하시겠습니까?\r\n모든 기록이 삭제될 수 있습니다.\r\n",
            "탈퇴", () =>
            {
                Utility.SetOffline();
                DeleteUser();

                // 구글 계정 로그아웃 처리 및 계정과 앱 연결 해제
                bool isGoogleUser = CYH_FirebaseManager.Auth.CurrentUser.ProviderData.Any(provider => provider.ProviderId == "google.com");
                if (isGoogleUser)
                {
                    GoogleSignIn.DefaultInstance.SignOut();
                    GoogleSignIn.DefaultInstance.Disconnect();
                }

                CYH_FirebaseManager.Auth.SignOut();
                PopupManager.Instance.HidePopup();
                SceneManager.LoadScene("LoginScene");
            },
            "취소", () => PopupManager.Instance.HidePopup()
           );
    }

    private void DeleteUser()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;

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
;
                Debug.Log("유저 삭제 성공");

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

                    CYH_FirebaseManager.Auth.SignOut();
                }

                // LoginScene으로 씬 전환
                OnClickDeleteAccount?.Invoke();
            });
    }
    #endregion
}
