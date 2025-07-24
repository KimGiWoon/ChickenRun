using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PlayLobbyPanel : UIBase, IInRoomCallbacks
{
    #region Serialized fields

    [Header("Top")]
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private Button _roomSettingButton;

    [Header("Player Slots (Fixed 8)")]
    [SerializeField] private List<PlayerSlot> _playerSlots;

    [Header("Map Select")]
    [SerializeField] private TMP_Text _mapNameText;
    [SerializeField] private Button _leftMapButton;
    [SerializeField] private Button _rightMapButton;

    [Header("Skin")]
    [SerializeField] private Button _skinSelectButton;

    [Header("Color Select")]
    [SerializeField] private Button _leftColorButton;
    [SerializeField] private Button _rightColorButton;
    [SerializeField] private Image _currentColorImage;

    [Header("Bottom")]
    [SerializeField] private Button _startOrReadyButton;
    [SerializeField] private TMP_Text _startOrReadyText;
    [SerializeField] private Button _backButton;

    #endregion // Serialized fields





    #region private fields

    private System.Action _onBack;
    private System.Action _onOpenRoomSetting;

    #endregion // private fields





    #region mono funcs

    private void Start()
    {
        _roomSettingButton.onClick.AddListener(() => _onOpenRoomSetting?.Invoke());
        _leftMapButton.onClick.AddListener(() => ChangeMap(-1));
        _rightMapButton.onClick.AddListener(() => ChangeMap(1));
        _skinSelectButton.onClick.AddListener(ChangeSkin);
        _leftColorButton.onClick.AddListener(() => ChangeColor(-1));
        _rightColorButton.onClick.AddListener(() => ChangeColor(1));
        _startOrReadyButton.onClick.AddListener(OnClickStartOrReady);
        _backButton.onClick.AddListener(() => _onBack?.Invoke());
    }

    /// <summary>
    /// 포톤 룸 속성이 업데이트되었을 때 호출됩니다.
    /// </summary>
    /// <param name="changedProperties"></param>
    public void OnRoomPropertiesUpdated(Hashtable changedProperties)
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion // mono funcs





    #region Photon callbacks

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Photon] 플레이어 입장: {newPlayer.NickName}");
        RefreshUI();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Photon] 플레이어 퇴장: {otherPlayer.NickName}");
        RefreshUI();
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("[Photon] 방 속성 변경됨");
        RefreshUI();
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"[Photon] 플레이어 속성 변경: {targetPlayer.NickName}");
        RefreshUI();
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[Photon] 방장이 변경됨: {newMasterClient.NickName}");
        RefreshUI();
    }

    public void OnLeftRoom()
    {
        Debug.Log("[Photon] 방에서 나감 (IInRoomCallbacks)");
    }

    #endregion // Photon callbacks





    #region public funcs

    public void Initialize(System.Action onBack, System.Action onOpenRoomSetting)
    {
        _onBack = onBack;
        _onOpenRoomSetting = onOpenRoomSetting;
    }

    public override void RefreshUI()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;

        _roomNameText.text = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("RoomName", out var value)
            ? value.ToString()
            : "이름 없음";
        _roomSettingButton.gameObject.SetActive(isMaster);
        _leftMapButton.gameObject.SetActive(isMaster);
        _rightMapButton.gameObject.SetActive(isMaster);

        RefreshPlayerSlots();
        RefreshBottomButton();
        RefreshMapDisplay();
        RefreshColorDisplay();
    }

    #endregion // public funcs





    #region private funcs

    private void RefreshPlayerSlots()
    {
        foreach (var slot in _playerSlots)
            slot.SetActive(false);

        var players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++) {
            _playerSlots[i].SetActive(true);
            _playerSlots[i].SetPlayer(players[i]);
        }
    }

    private void RefreshBottomButton()
    {
        if (PhotonNetwork.IsMasterClient) {
            bool allReady = CheckAllPlayersReady();
            _startOrReadyText.text = "게임 시작";
            _startOrReadyButton.interactable = allReady;
        }
        else {
            _startOrReadyText.text = "준비"; // TODO : 준비 상태에 따라 텍스트 변경
            _startOrReadyButton.interactable = true;
        }
    }

    private bool CheckAllPlayersReady()
    {
        return true; // TODO : 모든 플레이어의 준비 상태를 확인하는 로직 구현
    }

    private void OnClickStartOrReady()
    {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.LoadLevel("GameScene"); // TODO : 게임 씬 로드 로직 구현
        }
        else {
            // TODO : 개별 플레이어 준비 상태 토글
        }
    }

    private void ChangeMap(int dir)
    {
        // TODO : 맵 변경 로직 구현
    }

    private void ChangeSkin()
    {
        // TODO : 플레이어 스킨 선택 UI 표시
    }

    private void ChangeColor(int dir)
    {
        // TODO : 플레이어 색상 변경 로직 구현
    }

    private void RefreshMapDisplay()
    {
        _mapNameText.text = "맵"; // TODO : 맵 이미지로 교체
    }

    private void RefreshColorDisplay()
    {
        _currentColorImage.color = Color.green; // TODO : 현재 플레이어의 색상으로 교체
    }

    #endregion // private funcs
}
