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
            Utility.SetOffline();
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
