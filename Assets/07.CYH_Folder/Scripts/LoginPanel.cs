using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이메일/비밀번호 입력을 통한 로그인 기능을 담당하는 UI 패널 클래스
/// </summary>
public class LoginPanel : UIBase
{
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _signupButton;

    public Action OnClickSignup { get; set; }
    public Action OnClickLogin { get; set; }
    public Action MaxUser { get; set; }

    private void Start()
    {
        // 로그인 버튼 클릭 -> 로그인 옵션 팝업
        _loginButton.onClick.AddListener(() => OnClickLogin?.Invoke());

        // 회원가입 버튼 클릭 -> 회원가입 팝업
        _signupButton.onClick.AddListener(() =>
        {
            OnClickSignup?.Invoke();
        });
    }

    private async void OnEnable()
    {
        CurrentUserCount();
    }

    /// <summary>
    /// 현재 접속 인원을 확인하는 메서드
    /// 최대 인원을 초과한 경우 안내 팝업 호출
    /// </summary>
    public async void CurrentUserCount()
    {
        if (!await CurrentOnlineUserCount(20))
        {
            PopupManager.Instance.ShowOKPopup($"접속 인원이 초과되어 입장할 수 없습니다.\n(Max: 20명)", "OK", () => PopupManager.Instance.HidePopup());

            MaxUser?.Invoke();
        }
    }

    /// <summary>
    /// 현재 접속 중인 유저 수를 확인하고 최대 접속 인원 초과 여부를 반환하는 메서드
    /// </summary>
    /// <param name="maxOnlineUser">최대 접속 가능 인원 수 (기본값: 20)</param>
    /// <returns>접속 가능 여부 (true: 가능, false: 인원 초과)</returns>
    public async Task<bool> CurrentOnlineUserCount(int maxOnlineUser = 20)
    {
        DatabaseReference userDataRef = CYH_FirebaseManager.DataReference.Child("UserData");
        DataSnapshot snapshot = await userDataRef.GetValueAsync();

        int onlineCount = 0;
        foreach (var user in snapshot.Children)
        {
            var isOnlineValue = user.Child("IsOnline").Value;
            if (isOnlineValue != null && isOnlineValue.ToString() == "True")
            {
                onlineCount++;
            }
        }

        Debug.Log($"현재 접속 중인 인원 : {onlineCount}");
        return onlineCount < maxOnlineUser;
    }
}
