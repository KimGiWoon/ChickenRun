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
    [SerializeField] private TMP_Text _goldEggText;
    [SerializeField] private TMP_Text _normalEggText;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private Button _settingButton;

    [Header("Main UI")]
    [SerializeField] private List<UIBase> _uiList;

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
                findPanel.Initialize(OnBackToBase);
            }
            else if (ui is PlayLobbyPanel lobbyPanel) {
                lobbyPanel.Initialize(
                    onBack: OnBackToBase,
                    onOpenRoomSetting: OnClickRoomSetting
                );
            }
        }

        _settingButton.onClick.AddListener(OpenSettingPopup);

        // TODO: Firebase 닉네임 불러오기 예정
        //_nicknameText.text = FirebaseManager.Instance.CurrentUserNickname;

        UpdateTopCurrency(0, 0); // TODO: Firebase에서 불러오기.
        ShowUI(UIType.PlayBase); // 초기 UI 설정
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

    /// <summary>
    /// 상단에 있는 재화 UI, 예를 들어 골드 에그와 일반 에그의 수량을 업데이트합니다.
    /// </summary>
    /// <param name="goldEgg">골드에그 수량</param>
    /// <param name="normalEgg">일반에그 수량</param>
    public void UpdateTopCurrency(int goldEgg, int normalEgg)
    {
        _goldEggText.text = goldEgg.ToString();
        _normalEggText.text = normalEgg.ToString();
    }

    #endregion // public funcs





    #region private funcs

    private void OnClickPlayAlone()
    {
        string displayName = "닉네임" + Random.Range(1000, 9999); // TODO Firebase 닉네임 가져오기

        PhotonManager.Instance.SetOnJoinedRoomCallback(() => ShowUI(UIType.PlayLobby));

        // 커스텀 프로퍼티에 UI용 이름 포함
        ExitGames.Client.Photon.Hashtable customProps = new() {
        { "RoomName", displayName },
        { "Password", "" },
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

    private void OpenSettingPopup()
    {
        //PopupManager.Instance.ShowSettingPopup(); // TODO : 설정 팝업 구현 예정
    }

    #endregion // private funcs
}
