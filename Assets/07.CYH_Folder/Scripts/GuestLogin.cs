using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System;


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

        CYH_FirebaseManager.Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(async task =>
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

            Debug.Log("게스트 생성 완료");

            user.ReloadAsync();

            // 게스트 닉네임 변경 
            await Utility.SetGuestNickname(user);
            await user.ReloadAsync();

            Debug.Log("------유저 정보(GuestLogin)------");
            Debug.Log($"유저 닉네임 : {user.DisplayName}");
            Debug.Log($"유저 ID : {user.UserId}");
            Debug.Log($"이메일 : {user.Email}");

            // LoginPanel -> GameStartPanel 로 변경
            if (user != null)
            {
                Debug.Log("게스트 정보 업데이트 완료. GameStart패널 활성화");
                LoginCompleted?.Invoke();
            }
        });
    }
}
