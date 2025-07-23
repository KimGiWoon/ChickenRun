using System;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    #region Singleton

    public static PopupManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_popupPanel != null)
            _popupPanel.SetHide();
    }

    #endregion // Singleton





    #region serialized Fields

    [SerializeField] private PopupPanel _popupPanel;

    #endregion // serialized Fields





    #region public funcs

    public void ShowOKCancelPopup(string message,
                          string leftText = "OK", Action onLeftClick = null,
                          string rightText = "Cancel", Action onRightClick = null)
    {
        if (_popupPanel == null) {
            return;
        }

        _popupPanel.SetShow(message, leftText, onLeftClick, rightText, onRightClick);
    }

    public void ShowOKPopup(string message,
                      string leftText = "OK", Action onLeftClick = null)
    {
        if (_popupPanel == null) {
            return;
        }

        _popupPanel.SetShow(message, leftText, onLeftClick);
    }

    public void HidePopup()
    {
        _popupPanel?.SetHide();
    }

    #endregion // public funcs
}
