using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountPanel : UIBase
{
    [SerializeField] TMP_InputField _nicknameField;

    [SerializeField] Button _changeNicknameButton;
    [SerializeField] Button _editPasswordButton;
    [SerializeField] Button _deleteAccountButton;
    [SerializeField] Button _closePopupButton;

    [SerializeField] private string _checkedNickname;
    [SerializeField] private bool isEmailUser;

    //FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;
    public Action OnClickChangePassword { get; set; }
    public Action OnClickClosePopup { get; set; }


    private void Start()
    {
        _changeNicknameButton.onClick.AddListener(ChangeNickname);
        _editPasswordButton.onClick.AddListener(() => OnClickChangePassword?.Invoke());
        _deleteAccountButton.onClick.AddListener(OnClick_DelteButton);
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
    }

    private void OnEnable()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;

        // 닉네임 Placeholder의 기본 텍스트 = 현재 유저 닉네임
        _nicknameField.placeholder.GetComponent<TMP_Text>().text = currentUser.DisplayName;
    }

    /// <summary>
    /// 유저 닉네임을 변경하는 메서드
    /// </summary>
    private void ChangeNickname()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;
        _checkedNickname = _nicknameField.text;

        DatabaseReference userData = CYH_FirebaseManager.Database.RootReference.Child("UserData");

        userData.OrderByChild("Public/Nickname").EqualTo(_nicknameField.text).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("실패: " + task.Exception);
                PopupManager.Instance.ShowOKPopup("닉네임 중복체크 실패", "OK", () => PopupManager.Instance.HidePopup());

                return;
            }

            DataSnapshot snapshot = task.Result;

            if (!snapshot.Exists || snapshot.ChildrenCount == 0)
            {
                SetUserNickname(currentUser);
                PopupManager.Instance.ShowOKPopup("닉네임 재설정 완료", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }

            foreach (DataSnapshot user in snapshot.Children)
            {
                string uid = user.Key;
                string nickname = user.Child("Public").Child("Nickname").Value?.ToString();
                Debug.Log($"중복된 닉네임: {nickname}, uid: {uid}");

                // 팝업 (중복된 닉네임)
                PopupManager.Instance.ShowOKPopup("중복된 닉네임입니다.", "OK", () => PopupManager.Instance.HidePopup());
            }
        });
    }

    /// <summary>
    /// 유저 닉네임을 설정하는 메서드
    /// </summary>
    /// <param name="currentUser"></param>
    private void SetUserNickname(FirebaseUser currentUser)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = _nicknameField.text;

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

                // 팝업 (닉네임 변경 완료)
                PopupManager.Instance.ShowOKPopup("닉네임 변경 완료", "OK", () => PopupManager.Instance.HidePopup());
            });
    }

    //private void PasswordChange(FirebaseUser currentUser)
    //{
    //    currentUser.UpdatePasswordAsync(newPasswordInput.text)
    //        .ContinueWithOnMainThread(task =>
    //        {
    //            if (task.IsCanceled)
    //            {
    //                Debug.LogError("패스워드 변경 취소");
    //                editFailedPopup.SetActive(true);
    //                return;
    //            }

    //            if (task.IsFaulted)
    //            {
    //                Debug.LogError("패스워드 변경 실패");
    //                editFailedPopup.SetActive(true);
    //                return;
    //            }
    //            Debug.Log("패스워드 변경 성공");
    //            editSuccessPopup.SetActive(true);
    //        });
    //}

    //private void CheckPassword()
    //{
    //    FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;
    //    var credential = EmailAuthProvider.GetCredential(user.Email, oldPasswordInput.text);

    //    user.ReauthenticateAsync(credential)
    //        .ContinueWithOnMainThread(task =>
    //        {
    //            if (task.IsCanceled)
    //            {
    //                Debug.Log("재인증 취소");
    //                editFailedPopup.SetActive(true);
    //                return;
    //            }
    //            if (task.IsFaulted)
    //            {
    //                Debug.Log("패스워드 일치하지 않음");
    //                editFailedPopup.SetActive(true);
    //                return;
    //            }
    //            if (task.IsCompleted)
    //            {
    //                Debug.Log("패스워드 일치");

    //                if (oldPasswordInput.text == newPasswordInput.text)
    //                {
    //                    Debug.Log("새 패스워드가 기존 패스워드랑 같음");
    //                    editFailedPopup.SetActive(true);
    //                    return;
    //                }
    //                PasswordChange(user);
    //            }
    //        });
    //}

    /// <summary>
    /// 회원탈퇴 버튼 시 탈퇴 안내 팝업을 띄우는 메서드
    /// </summary>
    private void OnClick_DelteButton()
    {
        PopupManager.Instance.ShowOKCancelPopup("정말로 탈퇴하시겠습니까?\r\n모든 기록이 삭제될 수 있습니다.\r\n",
            "탈퇴", () => DeleteUser(), "취소", () => PopupManager.Instance.HidePopup());
    }

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

                Debug.Log("유저 삭제 성공");
                CYH_FirebaseManager.Auth.SignOut();
                PopupManager.Instance.ShowOKPopup("계정 삭제 완료", "OK", () => PopupManager.Instance.HidePopup());
            });
    }

    /// <summary>
    /// CurrentUser가 이메일 가입 계정인지 체크하는 메서드
    /// </summary>
    private void CheckEmailAccount()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        // 이메일 가입자인지 체크
        if (user != null)
        {
            foreach (IUserInfo info in user.ProviderData)
            {
                if (info.ProviderId == "password")
                {
                    isEmailUser = true;
                    break;
                }
            }
        }

        if (isEmailUser)
        {
            Debug.Log("이메일 가입 유저");
        }
        else
        {
            Debug.Log("이메일 가입 유저 아님");
            PopupManager.Instance.ShowOKPopup("닉네임 변경 불가 계정", "OK", () => PopupManager.Instance.HidePopup());
        }
    }

}
