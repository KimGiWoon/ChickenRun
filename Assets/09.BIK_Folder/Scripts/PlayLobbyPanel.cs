using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Button _mapInfoButton;

    [Header("Skin")]
    [SerializeField] private Button _skinSelectButton;
    [SerializeField] private SkinSelectPanel _skinSelectPanel;

    [Header("Color Select")]
    [SerializeField] private Button _leftColorButton;
    [SerializeField] private Button _rightColorButton;
    [SerializeField] private Image _currentColorImage;

    [Header("Bottom")]
    [SerializeField] private Button _startOrReadyButton;
    [SerializeField] private TMP_Text _startOrReadyText;
    [SerializeField] private Button _backButton;

    [Header("MapInfo")]
    [SerializeField] private GameInfoPanel _gameInfoPanel;

    #endregion // Serialized fields





    #region private fields

    private System.Action _onBack;
    private System.Action _onOpenRoomSetting;
    private MapType _currentMap = MapType.Map1;
    private ColorType _currentColor = ColorType.White;

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
        _mapInfoButton.onClick.AddListener(() => {
            _gameInfoPanel.SetShow(_currentMap);
        });
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
        PhotonManager.Instance.SetOnKickedCallback(_onBack);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion // mono funcs





    #region Photon callbacks

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Photon] 플레이어 입장: {GetPlayerDisplayName(newPlayer)}");
        RefreshUI();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Photon] 플레이어 퇴장: {GetPlayerDisplayName(otherPlayer)}");
        RefreshUI();
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        Debug.Log("[Photon] 방 속성 변경됨");
        RefreshUI();
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"[Photon] 플레이어 속성 변경: {GetPlayerDisplayName(targetPlayer)}");
        RefreshUI();
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[Photon] 방장이 변경됨: {GetPlayerDisplayName(newMasterClient)}");
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
            bool isReadyAlreadyTrue = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out var currentValue)
                                      && currentValue is bool b && b;

            if (!isReadyAlreadyTrue) {
                Hashtable props = new Hashtable {
                { "IsReady", true }
            };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }

            bool allReady = CheckAllPlayersReady();
            _startOrReadyText.text = "게임시작";
            _startOrReadyButton.interactable = allReady;
        }
        else {
            bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out var value)
                           && value is bool b && b;

            _startOrReadyText.text = isReady ? "준비 완료" : "준비";
            _startOrReadyButton.interactable = true;
        }
    }

    private bool CheckAllPlayersReady()
    {
        foreach (var player in PhotonNetwork.PlayerList) {
            if (player.IsMasterClient)
                continue;

            if (!player.CustomProperties.TryGetValue("IsReady", out var value) || !(value is bool b) || !b)
                return false;
        }

        return true;
    }

    private void OnClickStartOrReady()
    {
        if (PhotonNetwork.IsMasterClient) {
            // 모든 플레이어가 준비 완료됐는지 확인
            if (!CheckAllPlayersReady()) {
                Debug.LogWarning("[Photon] 모든 플레이어가 준비하지 않았습니다. 게임을 시작할 수 없습니다.");
                PopupManager.Instance.ShowOKPopup("모든 플레이어가 준비 상태여야 게임을 시작할 수 있습니다.", "확인");
                return;
            }

            // 맵 2일때만
            if (_currentMap == MapType.Map2) {
                var players = PhotonNetwork.PlayerList;
                Dictionary<string, int> colorCounts = new();

                foreach (var player in players) {
                    if (player.CustomProperties.TryGetValue("Color", out var colorObj) && colorObj is string colorStr) {
                        if (!colorCounts.ContainsKey(colorStr)) {
                            colorCounts[colorStr] = 0;
                        }
                        colorCounts[colorStr]++;
                    }
                }

                // 1. 같은 색상이 3명 이상인 경우 시작안됨.
                if (colorCounts.Values.Any(count => count >= 3)) {
                    PopupManager.Instance.ShowOKPopup("같은 색상을 3명 이상 선택할 수 없습니다.", "확인");
                    return;
                }

                // 2. 모두 다른 색깔이거나
                bool allUnique = colorCounts.Values.All(count => count == 1);
                if (allUnique) {
                    // Do nothing, 게임 시작 가능
                }
                else {
                    // 3. 같은 색상을 선택한 인원이 정확히 2명씩 있어야 팀전이 가능
                    bool allPairs = colorCounts.Values.All(count => count == 2);
                    bool playerCountEven = players.Length % 2 == 0;

                    if (!(allPairs && playerCountEven)) {
                        PopupManager.Instance.ShowOKPopup("같은 색상을 선택한 인원이 정확히 2명씩 있어야 팀전이 가능합니다.", "확인");
                        return;
                    }
                }
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            // 현재 맵 타입 가져오기
            string mapString = PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Map", out var value)
                ? value.ToString()
                : MapType.Map1.ToString(); // 기본값 Map1

            if (System.Enum.TryParse(mapString, out MapType selectedMap)) {
                Debug.Log($"[Photon] 게임 시작 - 선택된 맵: {selectedMap}");

                switch (selectedMap) {
                    case MapType.Map1:
                        PhotonNetwork.LoadLevel("GameScene_Map1");
                        break;
                    case MapType.Map2:
                        PhotonNetwork.LoadLevel("GameScene_Map2");
                        break;
                    case MapType.Map3:
                        PhotonNetwork.LoadLevel("GameScene_Map3");
                        break;
                    case MapType.Map4:
                        PhotonNetwork.LoadLevel("GameScene_Map4");
                        break;
                    default:
                        PhotonNetwork.LoadLevel("GameScene_Map1");
                        break;
                }
            }
            else {
                PhotonNetwork.LoadLevel("GameScene_Map1");
            }
        }
        else {
            bool isReady = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("IsReady", out var value) && value is bool b && b;

            Hashtable props = new Hashtable {
            { "IsReady", !isReady }
        };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    private void ChangeMap(int dir)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int mapCount = System.Enum.GetValues(typeof(MapType)).Length;
        int newIndex = ((int)_currentMap + dir + mapCount) % mapCount;
        _currentMap = (MapType)newIndex;

        Hashtable props = new() {
        { "Map", _currentMap.ToString() }};

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    private void ChangeSkin()
    {
        _skinSelectPanel.SetShow();
    }

    private void ChangeColor(int dir)
    {
        int count = System.Enum.GetValues(typeof(ColorType)).Length;
        int nextIndex = ((int)_currentColor + dir + count) % count;
        _currentColor = (ColorType)nextIndex;

        // Photon에 커스텀 프로퍼티로 저장
        Hashtable props = new Hashtable {
        { "Color", _currentColor.ToString() }};

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void RefreshMapDisplay()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Map", out var value)) {
            if (System.Enum.TryParse(value.ToString(), out MapType parsedMap)) {
                _currentMap = parsedMap;
                _mapNameText.text = _currentMap.ToString();
            }
            else {
                _mapNameText.text = "Unknown Map";
            }
        }
        else {
            _mapNameText.text = "Map1";
            _currentMap = MapType.Map1;
        }
    }

    private void RefreshColorDisplay()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Color", out var value)) {
            if (System.Enum.TryParse(value.ToString(), out ColorType parsedColor)) {
                _currentColor = parsedColor;
                _currentColorImage.color = Common.ConvertColorTypeToUnityColor(_currentColor);
            }
        }
    }

    private string GetPlayerDisplayName(Player player)
    {
        if (player.CustomProperties.TryGetValue("Nickname", out var nicknameObj) && nicknameObj is string nickname)
            return nickname;
        else
            return player.NickName; // fallback
    }

    #endregion // private funcs
}
