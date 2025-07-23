using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPanel : UIBase
{
    #region serialized fields

    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _leftButton;
    [SerializeField] private TMP_Text _leftButtonText;
    [SerializeField] private Button _rightButton;
    [SerializeField] private TMP_Text _rightButtonText;

    #endregion // serialized fields





    #region public funcs

    public void SetShow(string message,
                        string leftText, Action onLeftClick,
                        string rightText, Action onRightClick)
    {
        gameObject.SetActive(true);
        _messageText.text = message;

        _leftButton.gameObject.SetActive(true);
        _leftButtonText.text = leftText;
        _leftButton.onClick.RemoveAllListeners();
        _leftButton.onClick.AddListener(() => {
            onLeftClick?.Invoke();
            SetHide();
        });

        _rightButton.gameObject.SetActive(true);
        _rightButtonText.text = rightText;
        _rightButton.onClick.RemoveAllListeners();
        _rightButton.onClick.AddListener(() => {
            onRightClick?.Invoke();
            SetHide();
        });
    }

    public void SetShow(string message,
                        string leftText, Action onLeftClick)
    {
        gameObject.SetActive(true);
        _messageText.text = message;

        _leftButton.gameObject.SetActive(true);
        _leftButtonText.text = leftText;
        _leftButton.onClick.RemoveAllListeners();
        _leftButton.onClick.AddListener(() => {
            onLeftClick?.Invoke();
            SetHide();
        });

        _rightButton.gameObject.SetActive(false);
        _rightButton.onClick.RemoveAllListeners();
    }

    public override void SetHide()
    {
        base.SetHide();
        _messageText.text = "";
        _leftButton.onClick.RemoveAllListeners();
        _rightButton.onClick.RemoveAllListeners();
    }

    #endregion // public funcs
}
