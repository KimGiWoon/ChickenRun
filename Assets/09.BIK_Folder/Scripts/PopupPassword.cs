using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPassword : UIBase
{
    #region serialized fields

    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private Button _okButton;

    #endregion // serialized fields





    #region public funcs

    public void SetShow(Action onOKButton)
    {
        _okButton.onClick.AddListener(() => {
            if (string.IsNullOrEmpty(_passwordInputField.text)) {
                Debug.LogWarning("Password cannot be empty.");
                return;
            }
            PopupManager.Instance.CurrentPassword = _passwordInputField.text;
            SetHide();
            onOKButton?.Invoke();
        });
    }

    public override void SetHide()
    {
        base.SetHide();
        _passwordInputField.text = "";
    }

    #endregion // public funcs
}
