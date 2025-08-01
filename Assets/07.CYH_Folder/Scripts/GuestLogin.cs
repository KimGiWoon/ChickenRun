using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;


public class GuestLogin : MonoBehaviour
{
    [SerializeField] private Button _guestLoginButton;

    public Action LoginCompleted { get; set; }

    private void Start()
    {
        _guestLoginButton.onClick.AddListener(OnClick_GuestLogin);
    }

    private void OnClick_GuestLogin()
    {
        // 게스트 로그인 가능 여부 체크
        if(CYH_FirebaseManager.Auth.CurrentUser != null)
        {
            PopupManager.Instance.ShowOKPopup("게스트 로그인 불가", "OK", () => PopupManager.Instance.HidePopup());
            Debug.LogError($"유저 UID : {CYH_FirebaseManager.Auth.CurrentUser.UserId}  " +
                $"/ 유저 닉네임 : {CYH_FirebaseManager.Auth.CurrentUser.DisplayName}");
            return;
        }

        CYH_FirebaseManager.Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("게스트 로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"게스트 로그인 실패 / 원인: {task.Exception}");
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;

            FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
            //SetUserNickname(user);

            CYH_FirebaseManager.Instance.OnFirebaseLoginSuccess();

            // 새로고침
            user.ReloadAsync();

            Debug.Log("------유저 정보------");
            Debug.Log($"유저 닉네임 : {user.DisplayName}");
            Debug.Log($"유저 ID : {user.UserId}");
            Debug.Log($"이메일 : {user.Email}");

            // LoginPanel -> GameStartPanel 로 변경
            if (user != null)
            {
                Debug.Log("게스트 로그인 성공. GameStart패널 활성화");
                LoginCompleted?.Invoke();
            }
        });
    }

    ///// <summary>
    ///// 익명 유저 닉네임을 설정하는 메서드
    ///// </summary>
    ///// <param name="currentUser">닉네임을 설정할 유저</param>
    //private void SetUserNickname(FirebaseUser currentUser)
    //{
    //    UserProfile profile = new UserProfile();
    //    profile.DisplayName = CYH_FirebaseManager.CurrentUserNickname + UnityEngine.Random.Range(1000, 10000);

    //    currentUser.UpdateUserProfileAsync(profile)
    //        .ContinueWithOnMainThread(task =>
    //        {
    //            if (task.IsCanceled)
    //            {
    //                Debug.LogError("닉네임 설정 취소");
    //                return;
    //            }

    //            if (task.IsFaulted)
    //            {
    //                Debug.LogError("닉네임 설정 실패");
    //                return;
    //            }
    //            Debug.Log("닉네임 설정 성공");
    //        });
    //}
}
