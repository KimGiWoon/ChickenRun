using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneOptionPanel : UIBase
{
    #region serialized fields

    [SerializeField] Button _userButton;
    [SerializeField] Button _bugReportButton;
    [SerializeField] Slider _musicSlider;
    [SerializeField] Slider _effectSlider;

    #endregion // serialized fields





    #region mono funcs

    private void Start()
    {
        _userButton.onClick.AddListener(() => PopupManager.Instance.ShowAccountPanel());
        _bugReportButton.onClick.AddListener(() =>
        {
            Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSe6Q1V_gKJZvaKluFWdPRBpss0Rn6B5FnecEgl-s1lOxSwIjw/viewform?usp=sharing&ouid=100453097753956903851");
        });
        _musicSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetBGM(volume));
        _effectSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetSFX(volume));
    }

    #endregion // mono funcs
}
