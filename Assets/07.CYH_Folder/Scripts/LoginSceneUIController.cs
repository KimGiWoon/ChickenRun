using System.Collections.Generic;
using UnityEngine;

public class LoginSceneUIManager : MonoBehaviour
{
    private enum LoginUIType
    {
        Login,
        SocialPanel,
        SignUpPanel
    }

    [SerializeField] private List<UIBase> _uiList;

    private void Start()
    {
        ShowUI(LoginUIType.Login);

        foreach (var ui in _uiList)
        {
            if (ui is LoginPanel loginPanel)
            {
                loginPanel.OnClickSignup = () => ShowUI(LoginUIType.SignUpPanel);
                loginPanel.OnClickSocialLogin = () => ShowUI(LoginUIType.SocialPanel);
            }
            else if (ui is SignUpPanel signUpPanel)
            {
                
            }
            else if (ui is SocialLoginPanel socialPanel)
            {
                
            }
        }
    }

    private void ShowUI(LoginUIType type)
    {
        _uiList[(int)type].SetShow();
    }

    private void HideUI(LoginUIType type)
    {
        _uiList[(int)type].SetHide();
    }
}