using Firebase.Database;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel_LoginScene : UIBase
{
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _startText;
    [SerializeField] private float _blinkSecond;

    private int _maxOnlineUser = 20;

    private Coroutine _blinkCoroutine;

    public Action OnClickTouch { get; set; }


    private void Start()
    {
        _startButton.onClick.AddListener(() => 
        {
            Debug.Log($"_maxOnlineUser : {_maxOnlineUser}");
            CurrentUserCount();
        });
    }

    private void OnEnable()
    {
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private void OnDisable()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        _startText.enabled = true;
    }

    /// <summary>
    /// 현재 접속 인원을 확인하는 메서드
    /// 최대 인원을 초과한 경우 안내 팝업 호출
    /// </summary>
    private async void CurrentUserCount()
    {
        if (!await CurrentOnlineUserCount())
        {
            PopupManager.Instance.ShowOKPopup($"접속 인원이 초과되어 입장할 수 없습니다.\n(Max: {_maxOnlineUser}명)", "OK", () => PopupManager.Instance.HidePopup());
            return;
        }
        else
        {
            OnClickTouch.Invoke();
        }
    }

    /// <summary>
    /// 현재 접속 중인 유저 수를 확인하고 최대 접속 인원 초과 여부를 반환하는 메서드
    /// </summary>
    /// <returns>접속 가능 여부 (true: 가능, false: 인원 초과)</returns>
    private async Task<bool> CurrentOnlineUserCount()
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
        return onlineCount < _maxOnlineUser;
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            _startText.enabled = !_startText.enabled;
            yield return new WaitForSeconds(_blinkSecond);
        }
    }
}
