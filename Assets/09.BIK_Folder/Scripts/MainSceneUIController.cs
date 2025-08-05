using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneUIController : MonoBehaviour
{
    #region Serialized fields

    [Header("Top UI")]
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private Button _settingButton;

    [Header("Main UI")]
    [SerializeField] private List<UIBase> _uiList;

    [Header("Popup")]
    [SerializeField] private MainSceneOptionPanel _optionPanel;

    #endregion // Serialized fields





    #region private fields

    private UIBase _lastUI;
    private UIBase _currentUI;

    #endregion // private fields





    #region mono funcs

    /// <summary>
    /// UI들을 초기화하고, 각 UI에 필요한 콜백을 설정합니다.
    /// </summary>
    private void Start()
    {
        foreach (var ui in _uiList) {
            if (ui is PlayBasePanel basePanel) {
                basePanel.Initialize(
                    onClickPlayAlone: OnClickPlayAlone,
                    onClickMakeRoom: OnClickMakeRoom,
                    onClickGetIn: OnClickFindRoom,
                    onClickQuickMatch: OnClickQuickMatch
                );
            }
            else if (ui is PlayMakeRoomPanel makePanel) {
                makePanel.Initialize(onMake: OnMakeRoom, onBack: OnBackToLastUI);
            }
            else if (ui is PlayFindRoomPanel findPanel) {
                findPanel.Initialize(OnBackToBase, OnJoinRoom);
            }
            else if (ui is PlayLobbyPanel lobbyPanel) {
                lobbyPanel.Initialize(
                    onBack: OnBackToBase,
                    onOpenRoomSetting: OnClickRoomSetting
                );
            }
        }

        _settingButton.onClick.AddListener(OpenSettingPopup);

        _nicknameText.text = CYH_FirebaseManager.CurrentUserNickname;

        // 방에 들어와있는 경우(플레이씬에서 로비로 나왔을 때) 다시 로비로 자동 이동
        if (PhotonNetwork.InRoom) {
            ShowUI(UIType.PlayLobby);
        }
        else {
            ShowUI(UIType.PlayBase);
        }
    }

    #endregion // mono funcs





    #region public funcs

    /// <summary>
    /// 지정된 UI 타입에 해당하는 UI를 표시합니다.
    /// </summary>
    /// <param name="type">Common 스크립트에 있는 UIType 중 하나를 호출합니다.</param>
    public void ShowUI(UIType type)
    {
        foreach (var ui in _uiList) {
            ui.SetHide();
        }

        _lastUI = _currentUI;
        _currentUI = _uiList[(int)type];
        _currentUI.SetShow();
        _currentUI.RefreshUI();
    }

    /// <summary>
    /// 현재 표시 중인 UI를 새로 고칩니다.
    /// </summary>
    public void RefreshCurrentUI()
    {
        _currentUI?.RefreshUI();
    }

    #endregion // public funcs





    #region private funcs

    private void OnClickPlayAlone()
    {
        string displayName = CYH_FirebaseManager.CurrentUserNickname + Random.Range(1000, 9999);

        PhotonManager.Instance.SetOnJoinedRoomCallback(() => ShowUI(UIType.PlayLobby));

        // 커스텀 프로퍼티에 UI용 이름 포함
        ExitGames.Client.Photon.Hashtable customProps = new() {
        { "RoomName", displayName },
        { "Password", "" },
        { "Map", MapType.Map1.ToString() },
        { "MaxPlayersView", 1 }
    };

        RoomOptions options = new RoomOptions {
            MaxPlayers = 1,
            IsVisible = false,
            IsOpen = true,
            CustomRoomProperties = customProps,
            CustomRoomPropertiesForLobby = new[] { "RoomName", "Password", "MaxPlayersView" }
        };

        PhotonNetwork.CreateRoom(displayName, options);
    }

    private void OnClickMakeRoom()
    {
        ShowUI(UIType.PlayMakeRoom);
    }

    private void OnClickFindRoom()
    {
        ShowUI(UIType.PlayFindRoom);
    }

    private void OnClickQuickMatch()
    {
        PhotonManager.Instance.SetOnJoinedRoomCallback(() => ShowUI(UIType.PlayLobby));
        PhotonManager.Instance.JoinRandomRoomOrCreate();
    }

    private void OnBackToBase()
    {
        if (Photon.Pun.PhotonNetwork.InRoom) {
            var props = new ExitGames.Client.Photon.Hashtable {
            { "IsReady", false }
        };
            Photon.Pun.PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            PhotonManager.Instance.SetOnLeftRoomCallback(() => ShowUI(UIType.PlayBase));
            PhotonManager.Instance.LeaveRoom();
        }
        else {
            ShowUI(UIType.PlayBase);
        }
    }

    private void OnBackToLastUI()
    {
        if (_lastUI != null) {
            _currentUI.SetHide();
            _lastUI.SetShow();
            _lastUI.RefreshUI();
            _currentUI = _lastUI;
            _lastUI = null;
        }
        else {
            ShowUI(UIType.PlayBase);
        }
    }

    private void OnClickRoomSetting()
    {
        ShowUI(UIType.PlayMakeRoom);
    }

    private void OnMakeRoom()
    {
        if (Photon.Pun.PhotonNetwork.InRoom) {
            // 이미 방에 있는 상태면 바로 PlayLobby로 전환
            ShowUI(UIType.PlayLobby);
        }
        else {
            // 방 입장 후 콜백으로 PlayLobby로 전환
            PhotonManager.Instance.SetOnJoinedRoomCallback(() => ShowUI(UIType.PlayLobby));
        }
    }

    private void OnJoinRoom()
    {
        PhotonManager.Instance.SetOnJoinedRoomCallback(() => ShowUI(UIType.PlayLobby));
    }

    private void OpenSettingPopup()
    {
        _optionPanel.SetShow();
    }

    #endregion // private funcs
}
