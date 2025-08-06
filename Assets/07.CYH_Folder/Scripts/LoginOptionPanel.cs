using System;
using UnityEngine;
using UnityEngine.UI;


public class LoginOptionPanel : UIBase
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _emailLoginButton;
    
    public Action OnClickClosePopup { get; set; }
    public Action OnClickEmailLogin { get; set; }

    private void Start()
    {
        _closeButton.onClick.AddListener(() => OnClickClosePopup?.Invoke());
        _emailLoginButton.onClick.AddListener(() => OnClickEmailLogin?.Invoke());
    }
}
