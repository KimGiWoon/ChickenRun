using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class NicknameChangePanel : UIBase
{
    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _nicknameChangeButton;

    [SerializeField] private TMP_InputField _nicknameField;

    [SerializeField] private string _currentNickname;

    public Action OnClickClosePopup;
    public Action OnClickNicknameChange;
    public Action OnClickNicknameChange_Success;

    private void Start()
    {
        // 닉네임 글자 수 제한 (6글자)
        _nicknameField.characterLimit = 6;

        // 팝업 닫기 버튼
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
        _nicknameChangeButton.onClick.AddListener(ChanegeNickname);
    }

    private void OnEnable()
    {
        FirebaseUser user = CYH_FirebaseManager.Auth.CurrentUser;

        _currentNickname = user.DisplayName;

        // placeholder 텍스트 = 유저 현재 닉네임
        _nicknameField.placeholder.GetComponent<TMP_Text>().text = user.DisplayName;
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

    private async void ChanegeNickname()
    {
        if (string.IsNullOrEmpty(_nicknameField.text.Trim()))
        {
            ShowPopup("닉네임을 입력해주세요.");
            return;
        }

        // 닉네임 글자 수 체크
        if (_nicknameField.characterLimit > 6)
        {
            ShowPopup("닉네임은 6글자 이내로 입력해 주세요.");
            return;
        }

        // 기존 닉네임 일치 여부 체크
        if (_currentNickname == _nicknameField.text)
        {
            Debug.LogError("동일 닉네임");
            ShowPopup("기존에 사용 중인 닉네임과 동일합니다.\r\n다른 닉네임을 입력해 주세요.");
            return;
        }

        // 닉네임 DB와 중복 체크
        bool available = await IsNicknameAvailableAsync();
        if (!available)
        {
            ShowPopup("중복된 닉네임입니다.");
            return;
        }

        //NicknameCheck();

        // 닉네임 재설정 및 데이터베이스에 저장
        Utility.SetNickname(_nicknameField.text);

        ShowPopup("닉네임 변경 성공");


        PopupManager.Instance.ShowOKPopup("닉네임 변경 성공", "OK", () =>
        {
            PopupManager.Instance.HidePopup();
            
            // 닉네임 변경 패널 비활성화
            OnClickNicknameChange_Success?.Invoke();
        });

        //PopupManager.Instance.ShowOKPopup("닉네임 변경 성공.\r\n다시 로그인해 주세요.", "OK", () =>
        //{
        //    PopupManager.Instance.HidePopup();

        //    // 닉네임 패널 비활성화
        //    OnClickNicknameChange?.Invoke();

        //    // 강제 로그아웃
        //    Utility.SetOffline();
        //    CYH_FirebaseManager.Auth.SignOut();

        //    // 로그인 씬 전환
        //    SceneManager.LoadScene("LoginScene");
        //});
    }

    /// <summary>
    /// 회원가입 시 유저 닉네임을 중복 체크하는 메서드
    /// </summary>
    private void NicknameCheck()
    {
        DatabaseReference userData = CYH_FirebaseManager.Database.RootReference.Child("UserData");

        userData.OrderByChild("Nickname").EqualTo(_nicknameField.text).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("실패: " + task.Exception);
                ShowPopup("DB 접근 실패");
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (!snapshot.Exists || snapshot.ChildrenCount == 0)
            {
                Debug.Log("사용 가능한 닉네임");
            }

            foreach (DataSnapshot user in snapshot.Children)
            {
                string uid = user.Key;
                string nickname = user.Child("Nickname").Value?.ToString();
                Debug.Log($"중복된 닉네임: {nickname}, uid: {uid}");

                // 팝업 (중복된 닉네임)
                ShowPopup("중복된 닉네임입니다.");
                return;
            }
        });
    }

    /// <summary>
    /// 회원가입 시 유저 닉네임을 중복 체크하는 메서드
    /// </summary>
    private async Task<bool> IsNicknameAvailableAsync()
    {
        var userData = CYH_FirebaseManager.Database.RootReference.Child("UserData");
        var snapshot = await userData.OrderByChild("Nickname").EqualTo(_nicknameField.text).GetValueAsync();

        if (!snapshot.Exists || snapshot.ChildrenCount == 0) return true;

        string uid = CYH_FirebaseManager.Auth.CurrentUser.UserId;
        foreach (var user in snapshot.Children)
        {
            if (user.Key != uid) return false;
        }
        return true;
    }
}
