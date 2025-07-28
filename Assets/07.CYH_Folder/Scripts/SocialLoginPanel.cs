using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;

public class SocialLoginPanel : UIBase
{
    [SerializeField] private Button _closePopupButton;

    public Action OnClickClosePopup { get; set; }

    private void Start()
    {
        {
            _closePopupButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
        }
    }
}