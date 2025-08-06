using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun.Demo.Cockpit;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
}
