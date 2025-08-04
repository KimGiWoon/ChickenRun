using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LinkPanel : UIBase
{
    //[SerializeField] private TMP_InputField _nicknameField;   
    //[SerializeField] private TMP_InputField _emailField;      
    //[SerializeField] private TMP_InputField _passwordField;   

    [SerializeField] private Button _googleButton;
    [SerializeField] private Button _emailButton;
    [SerializeField] private Button _closeButton;

    [SerializeField] private GameStartPanel _gameStartPanel;

    public Action OnClickLinkWithEmail { get; set; }
    public Action OnClickClosePopup { get; set; }
    public Action OnClickSignOut { get; set; }


    private void Start()
    {
        _googleButton.onClick.AddListener(OnClick_LinkWithGoogle);
        _emailButton.onClick.AddListener(() => OnClickLinkWithEmail?.Invoke());
        _closeButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
    }

    /// 익명 계정을 이메일 가입 계정으로 전환
    /// </summary>
    //private void OnClick_LinkWithEmail_Set()
    //{
    //    string email = _emailField.text;
    //    string password = _passwordField.text;

    //    // 익명 로그인 유저 체크
    //    FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
    //    if (user == null || !user.IsAnonymous)
    //    {
    //        Debug.LogWarning("익명 로그인 유저x");
    //        return;
    //    }

    //    Credential credential = EmailAuthProvider.GetCredential(email, password);

    //    user.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
    //    {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogError("계정 전환 취소");
    //            return;
    //        }
    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError($"계정 전환 실패 / 원인: {task.Exception}");

    //            PopupManager.Instance.ShowOKPopup("이메일 계정으로 전환 실패", "OK", () => PopupManager.Instance.HidePopup());
    //            return;
    //        }

    //        Debug.Log("익명 계정 -> 이메일 계정 전환 성공");

    //        FirebaseUser linkedUser = task.Result.User;
    //        SetUserNickname(linkedUser);

    //        user.ReloadAsync();

    //        PopupManager.Instance.ShowOKPopup("이메일 계정으로 전환 성공\r\n 다시 로그인 해주세요.", "OK", () => PopupManager.Instance.HidePopup());

    //        Debug.Log("이메일 가입 성공");
    //        Debug.Log("------유저 정보------");
    //        Debug.Log($"유저 닉네임 : {user.DisplayName}");
    //        Debug.Log($"유저 ID : {user.UserId}");
    //        Debug.Log($"이메일 : {user.Email}");
    //    });
    //}


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

            CYH_FirebaseManager.Auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWithOnMainThread(linkTask =>
            {
                if (linkTask.IsCanceled)
                {
                    PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 취소", "OK", () => PopupManager.Instance.HidePopup());
                    return;
                }

                if (linkTask.IsFaulted)
                {
                    PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 실패", "OK", () => PopupManager.Instance.HidePopup());
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

                // 구글 계정 닉네임으로 currentUser을 닉네임으로 변경
                //SetNickname(user, googleDisplayName);
                Utility.SetNickname(user, googleDisplayName);

                Debug.Log("------유저 정보------");
                Debug.Log($"유저 이름 : {Utility.LoadNickname()}");
                Debug.Log($"유저 ID: {user.UserId}");
                Debug.Log($"이메일 : {user.Email}");

                //StartCoroutine(WaitForReloadAndLog(user));

                PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 성공", "OK", () => PopupManager.Instance.HidePopup());

                //PopupManager.Instance.ShowOKPopup("구글 계정으로 전환 성공\r\n 다시 로그인 해주세요.", "OK", () =>
                //{
                //    PopupManager.Instance.HidePopup();

                //    // SignOut
                //    CYH_FirebaseManager.Auth.SignOut();

                //    // LoginPanel ShowUI, GameStartPanel HideUI
                //    OnClickSignOut?.Invoke();
                //});
            });
        });
    }


    /// <summary>
    /// 익명계정에서 구글계정으로 전환된 유저의 DisplayName을 구글계정 DisplayName으로 전환하는 메서드 
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    /// <param name="googleDisplayName">새로 설정할 닉네임(구글 계정 닉네임)</param>
    private void SetNickname(FirebaseUser currentUser, string googleDisplayName)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = googleDisplayName;

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("닉네임 설정 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("닉네임 설정 실패");
                    return;
                }

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");

                _gameStartPanel.OnSetNicknameField?.Invoke(googleDisplayName);
            });
    }

    private IEnumerator WaitForReloadAndLog(FirebaseUser user)
    {
        var reloadTask = user.ReloadAsync();
        yield return new WaitUntil(() => reloadTask.IsCompleted);
    }
}