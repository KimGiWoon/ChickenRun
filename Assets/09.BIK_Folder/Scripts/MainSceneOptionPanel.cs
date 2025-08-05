using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneOptionPanel : UIBase
{
    #region serialized fields

    [SerializeField] Button _userButton;
    [SerializeField] Slider _musicSlider;
    [SerializeField] Slider _effectSlider;

    #endregion // serialized fields





    #region mono funcs

    private void Start()
    {
        _userButton.onClick.AddListener(() => PopupManager.Instance.ShowAccountPanel());
        _musicSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetBGM(volume));
        _effectSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetSFX(volume));
    }

    #endregion // mono funcs
}
