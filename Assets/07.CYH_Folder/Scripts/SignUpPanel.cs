using Firebase.Auth;
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
        _emailField.text = "";
        _passwordField.text = "";
    }

    /// <summary>
    /// 회원가입 패널의 가입하기 버튼 클릭 시 호출되는 메서드
    /// 입력한 이메일/비밀번호로 Firebase 가입 시도
    /// 가입하기 클릭 시: 중복체크/패스워드 일치 여부 검사 후 인증 이메일 전송
    /// 회원가입 성공: 인증 메일 발송 팝업 활성화
    /// 회원가입 실패: 에러 코드 출력
    /// </summary>
    public void OnClick_SignUp()
    {
        // 닉네임 미입력
        if (_nicknameField.text == "")
        {
            Debug.LogError("닉네임 미입력");

            // 팝업 (패스워드 입력 요청)
            PopupManager.Instance.ShowOKPopup("닉네임을 입력해 주세요.", "OK", () => PopupManager.Instance.HidePopup());

            return;
        }

        // 닉네임 중복 체크 재검사
        if (_nicknameField.text != _checkedNickname)
        {
            Debug.LogError("닉네임 중복 체크 필요");

            // 팝업 (패스워드 입력 요청)
            PopupManager.Instance.ShowOKPopup("닉네임을 중복체크를 해주세요.", "OK", () => PopupManager.Instance.HidePopup());

            return;
        }

        // 이메일 중복 체크 재검사
        if (_checkedEmail != _emailField.text)
        {
            _checkedEmail = "";

            Debug.LogError("이메일 중복 체크 필요");

            // 팝업 (이메일 중복 체크)
            PopupManager.Instance.ShowOKPopup("이메일 중복체크를 해주세요.", "OK", () => PopupManager.Instance.HidePopup());

            return;
        }

        // 패스워드 미입력
        if (_passwordField.text == "" || _passwordConfirmField.text == "")
        {
            Debug.LogError("패스워드 필드 비어있음");

            // 팝업 (패스워드 입력 요청)
            PopupManager.Instance.ShowOKPopup("패스워드를 입력해 주세요.", "OK", () => PopupManager.Instance.HidePopup());

            return;
        }

        // 패스워드 일치 여부 체크
        if (_passwordField.text != _passwordConfirmField.text)
        {
            Debug.LogError("패스워드 일치하지 않음");

            // 팝업 (패스워드 불일치)
            PopupManager.Instance.ShowOKPopup("패스워드가 일치하지 않습니다.", "OK", () => PopupManager.Instance.HidePopup());

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
                    // 팝업 (회원가입 실패)
                    PopupManager.Instance.ShowOKPopup("회원가입 실패", "OK", () => PopupManager.Instance.HidePopup());

                    Debug.LogError($"가입 실패 / 원인: {task.Exception}");
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
                    PopupManager.Instance.ShowOKPopup("인증 메일이 발송되었습니다.", "OK", () => PopupManager.Instance.HidePopup());

                    // 새로고침
                    user.ReloadAsync();

                    Debug.Log("이메일 가입 성공");
                    Debug.Log("------유저 정보------");
                    Debug.Log($"유저 이름 : {user.DisplayName}");
                    Debug.Log($"유저 ID: {user.UserId}");
                    Debug.Log($"이메일 : {user.Email}");
                }
            });
    }

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
                     // 팝업 (이메일 형식 확인)
                     PopupManager.Instance.ShowOKPopup("올바른 이메일 형식이 아닙니다.", "OK", () => PopupManager.Instance.HidePopup());

                     Debug.LogError($"이메일 중복 체크 실패 / 원인 : {task.Exception}");
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
                         Debug.Log("중복된 이메일");

                         // 팝업 (중복된 이메일)
                         PopupManager.Instance.ShowOKPopup("중복된 이메일입니다.", "OK", () => PopupManager.Instance.HidePopup());
                     }

                     else
                     {
                         Debug.Log("사용 가능한 이메일입니다.");

                         // 팝업 (사용 가능 이메일)
                         PopupManager.Instance.ShowOKPopup("사용가능한 이메일입니다.", "OK", () => PopupManager.Instance.HidePopup());
                     }
                 }
             });
    }

    /// <summary>
    /// 회원가입 인증 이메일을 전송하는 메서드
    /// </summary>
    /// <param name="currentUser">인증 이메일을 보낼 유저</param>
    private void SendEmailVerification(FirebaseUser currentUser)
    {
        currentUser.SendEmailVerificationAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("인증 이메일 전송 취소");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log($"인증 이메일 전송 실패 / 원인 : {task.Exception}");
                    return;
                }
                Debug.Log("인증 이메일 전송 완료");
            });
    }

    private void NicknameCheck()
    {
        _checkedEmail = _nicknameField.text;

    }

    /// <summary>
    /// 회원가입 시 유저 닉네임을 설정하는 메서드
    /// </summary>
    /// <param name="currentUser">닉네임을 설정할 유저</param>
    private void SetUserNickname(FirebaseUser currentUser)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = _nicknameField.text;

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("닉네임 변경 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("닉네임 변경 실패");
                    return;
                }
                Debug.Log("닉네임 변경 성공");
            });
    }
}
