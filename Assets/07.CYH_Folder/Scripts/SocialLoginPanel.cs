using System;
using UnityEngine;
using UnityEngine.UI;


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