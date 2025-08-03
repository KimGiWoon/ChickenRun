using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordChangePanel : UIBase
{
    [SerializeField] private Button _closePopupButton;
    [SerializeField] private Button _nicknameChaneButton;
    [SerializeField] private Button _passwordChangeButton;
    [SerializeField] private Button _deleteAccountButton;

    public Action OnClickClosePopup;
    public Action OnClickNicknameChange;
    public Action OnClickPasswordChange;
    public Action OnClickDeleteAccount;
    public Action OnClickSignOut;

    private void Start()
    {
        // 팝업 닫기 버튼
        _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
    }
}
