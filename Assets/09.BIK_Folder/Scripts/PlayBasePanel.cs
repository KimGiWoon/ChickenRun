using UnityEngine;
using UnityEngine.UI;

public class PlayBasePanel : UIBase
{
    #region // Serialized fields

    [SerializeField] private Button _playAloneButton;
    [SerializeField] private Button _makeRoomButton;
    [SerializeField] private Button _getInButton;
    [SerializeField] private Button _quickMatchButton;

    #endregion // Serialized fields





    #region private fields

    private System.Action _onClickPlayAlone;
    private System.Action _onClickMakeRoom;
    private System.Action _onClickGetIn;
    private System.Action _onClickQuickMatch;

    #endregion // private fields





    #region mono funcs

    private void Start()
    {
        _playAloneButton.onClick.AddListener(() => _onClickPlayAlone?.Invoke());
        _makeRoomButton.onClick.AddListener(() => _onClickMakeRoom?.Invoke());
        _getInButton.onClick.AddListener(() => _onClickGetIn?.Invoke());
        _quickMatchButton.onClick.AddListener(() => _onClickQuickMatch?.Invoke());
    }

    #endregion // mono funcs





    #region public funcs

    public void Initialize(
        System.Action onClickPlayAlone,
        System.Action onClickMakeRoom,
        System.Action onClickGetIn,
        System.Action onClickQuickMatch)
    {
        _onClickPlayAlone = onClickPlayAlone;
        _onClickMakeRoom = onClickMakeRoom;
        _onClickGetIn = onClickGetIn;
        _onClickQuickMatch = onClickQuickMatch;
    }

    #endregion // public funcs
}
