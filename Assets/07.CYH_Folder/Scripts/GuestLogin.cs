using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;

public class GuestLogin : MonoBehaviour
{
    [SerializeField] private Button _guestLoginButton;

    private void Start()
    {
        _guestLoginButton.onClick.AddListener(OnClick_GuestLogin);
    }

    private void OnClick_GuestLogin()
    {
        CYH_FirebaseManager.Auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
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
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

           FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;

            Debug.Log("게스트 로그인 성공");
            Debug.Log("------유저 정보------");
            Debug.Log($"유저 닉네임 : {user.DisplayName}");
            Debug.Log($"유저 ID : {user.UserId}");
            Debug.Log($"이메일 : {user.Email}");

        });
    }
}
