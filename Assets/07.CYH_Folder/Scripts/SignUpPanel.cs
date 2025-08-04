using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 닉네임/이메일/비밀번호 입력을 통한 회원가입 기능을 담당하는 UI 패널 클래스
/// </summary>
public class SignUpPanel : UIBase
{
    [SerializeField] private TMP_InputField _nicknameField;
    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _passwordField;
    [SerializeField] private TMP_InputField _passwordConfirmField;

    [SerializeField] private Button _nicknameCheckButton;
    [SerializeField] private Button _emailCheckButton;
    [SerializeField] private Button _signUpButton;
    [SerializeField] private Button _closePopupButton;

    [SerializeField] private string _checkedEmail;
    [SerializeField] private string _checkedNickname;

    public Action OnClickSignup { get; set; }
    public Action OnClickClosePopup { get; set; }
    public Action OnClickNicknameCheck { get; set; }
    public Action OnClickEmailCheck { get; set; }


    private void Start()
    {
        // 닉네임 글자 수 제한 (6글자)
        _nicknameField.characterLimit = 6;

        _nicknameCheckButton.onClick.AddListener(NicknameCheck);
        _emailCheckButton.onClick.AddListener(EmailCheck);
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
        _signUpButton.onClick.AddListener(() =>
        {
            OnClick_SignUp();
            OnClickSignup?.Invoke();
        });
    }

    public override void RefreshUI()
    {
        _nicknameField.text = "";
        _emailField.text = "";
        _passwordField.text = "";
        _passwordConfirmField.text = "";
    }

    /// <summary>
    /// 안내 메세지 팝업을 띄우고 닫는 메서드
    /// </summary>
    /// <param name="message">팝업에 표시할 안내 메세지</param>
    private void ShowPopup(string message)
    {
        //Debug.LogError(message);
        PopupManager.Instance.ShowOKPopup(message, "OK", () => PopupManager.Instance.HidePopup());
    }

    #region signup

    /// <summary>
    /// 회원가입 패널의 가입하기 버튼 클릭 시 호출되는 메서드
    /// 입력한 이메일/비밀번호로 Firebase 가입 시도
    /// 가입하기 클릭 시: 중복체크/패스워드 일치 여부 검사 후 인증 이메일 전송
    /// 회원가입 성공: 인증 메일 발송 팝업 활성화
    /// 회원가입 실패: 에러 코드 출력
    /// 회원가입 실패 팝업: 1.닉네임 미입력 2.닉네임 중복체크 재검사 3.이메일 미입력 
    ///                    4.이메일 중복 체크 재검사 5.패스워드 미입력 6.패스워드 일치 여부 체크
    /// </summary>
    public void OnClick_SignUp()
    {
        // 닉네임 미입력
        if (string.IsNullOrEmpty(_nicknameField.text))
        {
            ShowPopup("닉네임을 입력해 주세요.");
            return;
        }

        // 닉네임 중복 체크 재검사
        if (_nicknameField.text != _checkedNickname)
        {
            ShowPopup("닉네임 중복체크를 해주세요.");
            return;
        }

        // 이메일 중복 체크 재검사
        if (_checkedEmail != _emailField.text)
        {
            _checkedEmail = "";

            ShowPopup("이메일 중복체크를 해주세요.");
            return;
        }

        // 패스워드 미입력
        if (string.IsNullOrEmpty(_passwordField.text) || string.IsNullOrEmpty(_passwordConfirmField.text))
        {
            ShowPopup("패스워드를 입력해 주세요.");
            return;
        }

        // 패스워드 일치 여부 체크
        if (_passwordField.text != _passwordConfirmField.text)
        {
            ShowPopup("패스워드가 일치하지 않습니다.");
            return;
        }

        // 유저 등록
        CYH_FirebaseManager.Auth.CreateUserWithEmailAndPasswordAsync(_emailField.text, _passwordField.text)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("이메일 가입 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError($"가입 실패 / 원인: {task.Exception}");

                    // 팝업 (회원가입 실패)
                    ShowPopup("회원가입 실패");
                    return;
                }

                if (task.IsCompletedSuccessfully)
                {
                    AuthResult result = task.Result;
                    FirebaseUser user = result.User;

                    // 유저 이름 등록
                    SetUserNickname(user);

                    // 인증 이메일 전송
                    SendEmailVerification(user);

                    // 팝업 (이메일 전송)
                    ShowPopup("인증 메일이 발송되었습니다.");

                    // 새로고침
                    user.ReloadAsync();

                    Debug.Log("이메일 가입 성공");
                    Debug.Log("------유저 정보------");
                    Debug.Log($"유저 닉네임 : {user.DisplayName}");
                    Debug.Log($"유저 ID : {user.UserId}");
                    Debug.Log($"이메일 : {user.Email}");
                }
            });
    }

    #endregion

    #region email

    /// <summary>
    /// 회원가입 시 이메일 중복 체크하는 메서드
    /// </summary>
    private void EmailCheck()
    {
        _checkedEmail = _emailField.text;

        CYH_FirebaseManager.Auth.FetchProvidersForEmailAsync(_emailField.text)
             .ContinueWithOnMainThread(task =>
             {

                 if (task.IsCanceled)
                 {
                     Debug.LogError($"이메일 중복 체크 취소");
                     return;
                 }

                 else if (task.IsFaulted)
                 {
                     Debug.LogError($"이메일 중복 체크 실패 / 원인 : {task.Exception}");
                     
                     // 팝업 (이메일 형식 확인)
                     ShowPopup("올바른 이메일 형식이 아닙니다.");
                     return;
                 }

                 else if (task.IsCompletedSuccessfully)
                 {
                     var providers = task.Result;

                     foreach (string provider in task.Result)
                     {
                         Debug.Log(provider);
                     }

                     if (providers.Any())
                     {
                         // 팝업 (중복된 이메일)
                         ShowPopup("중복된 이메일입니다.");
                     }

                     else
                     {
                         // 팝업 (사용 가능 이메일)
                         ShowPopup("사용 가능한 이메일입니다.");
                     }
                 }
             });
    }

    /// <summary>
    /// 회원가입 인증 메일을 전송하는 메서드
    /// </summary>
    /// <param name="currentUser">인증 메일을 보낼 유저</param>
    private void SendEmailVerification(FirebaseUser currentUser)
    {
        currentUser.SendEmailVerificationAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("인증 메일 전송 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.Log($"인증 메일 전송 실패 / 원인 : {task.Exception}");
                    return;
                }
                Debug.Log("인증 메일 전송 완료");
            });
    }

    #endregion

    #region nickname

    /// <summary>
    /// 회원가입 시 유저 닉네임을 중복 체크하는 메서드
    /// </summary>
    private void NicknameCheck()
    {
         _checkedNickname = _nicknameField.text;

        //DatabaseReference userData = CYH_FirebaseManager.Database.GetReference("UserData");
        DatabaseReference userData = CYH_FirebaseManager.Database.RootReference.Child("UserData");

        userData.OrderByChild("NickName").EqualTo(_nicknameField.text).GetValueAsync().ContinueWithOnMainThread(task=>
        {
            if (task.IsFaulted)
            {
                Debug.Log("실패: " + task.Exception);
                return;
            }
            
            DataSnapshot snapshot = task.Result;
            
            if (!snapshot.Exists || snapshot.ChildrenCount == 0)
            {
                // 팝업 (사용 가능 닉네임)
                ShowPopup("사용 가능한 닉네임입니다.");
                return;
            }

            foreach (DataSnapshot user in snapshot.Children)
            {
                string uid = user.Key;
                string nickname = user.Child("NickName").Value?.ToString();
                Debug.Log($"중복된 닉네임: {nickname}, uid: {uid}");

                // 팝업 (중복된 닉네임)
                ShowPopup("중복된 닉네임입니다.");
            }
        });
    }

    /// <summary>
    /// 회원가입 시 유저 닉네임을 설정하는 메서드
    /// </summary>
    /// <param name="currentUser">닉네임을 설정할 유저</param>
    private void SetUserNickname(FirebaseUser currentUser)
    {
        DatabaseReference userData = CYH_FirebaseManager.Database.RootReference.Child("UserData");
        
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
                // TODO: <최연호> 닉네임 UserData UpdateChildrenAsync / SetValueAsync
                // RankData SetValue
            });
    }
}

#endregion