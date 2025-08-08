using Firebase.Auth;
using System;
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

    private void ChanegeNickname()
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

        // 닉네임 재설정 및 데이터베이스에 저장
        //SetNickname();
        Utility.SetNickname(_nicknameField.text);

        PopupManager.Instance.ShowOKPopup("닉네임 변경 성공.\r\n다시 로그인해 주세요.", "OK", () =>
        {
            PopupManager.Instance.HidePopup();

            // 닉네임 패널 비활성화
            OnClickNicknameChange?.Invoke();

            // 강제 로그아웃
            CYH_FirebaseManager.Auth.SignOut();

            // 로그인 씬 전환
            SceneManager.LoadScene("LoginScene");
        });
    }
}