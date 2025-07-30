using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayMakeRoomPanel : UIBase
{
    #region Serialized fields

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private TMP_InputField _passwordInput;

    [Header("Max Player Setting")]
    [SerializeField] private Button _leftArrowButton;
    [SerializeField] private Button _rightArrowButton;
    [SerializeField] private TextMeshProUGUI _maxPlayerText;

    [Header("Action Buttons")]
    [SerializeField] private Button _createButton;
    [SerializeField] private TMP_Text _createButtonText;
    [SerializeField] private Button _cancelButton;

    #endregion // Serialized fields





    #region private fields

    private System.Action _onMake;
    private System.Action _onBack;
    private int _maxPlayers = 4;

    #endregion // private fields





    #region mono funcs

    private void Start()
    {
        _leftArrowButton.onClick.AddListener(() => UpdateMaxPlayers(-1));
        _rightArrowButton.onClick.AddListener(() => UpdateMaxPlayers(1));
        _createButton.onClick.AddListener(OnClick_CreateRoom);
        _cancelButton.onClick.AddListener(() => _onBack?.Invoke());

        UpdateMaxPlayers(0); // 초기 표시
    }

    #endregion // mono funcs





    #region public funcs

    public void Initialize(System.Action onMake, System.Action onBack)
    {
        _onMake = onMake;
        _onBack = onBack;
    }

    #endregion // public funcs





    #region private funcs

    private void UpdateMaxPlayers(int delta)
    {
        _maxPlayers = Mathf.Clamp(_maxPlayers + delta, 1, 8);
        _maxPlayerText.text = _maxPlayers.ToString();
    }

    private void OnClick_CreateRoom()
    {
        string roomName = _roomNameInput.text.Trim();
        string password = _passwordInput.text.Trim();

        if (string.IsNullOrEmpty(roomName)) {
            PopupManager.Instance.ShowOKPopup("방 이름을 입력해주세요.", "확인");
            return;
        }

        _onMake?.Invoke();

        if (Photon.Pun.PhotonNetwork.InRoom) {
            // 방 안에 있으면 커스텀 속성만 갱신
            ExitGames.Client.Photon.Hashtable newProps = new() {
            { "RoomName", roomName },
            { "Password", password },
            { "Map", "Map1" },
        };

            Photon.Pun.PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);
        }
        else {
            // 방 밖에 있으면 새로 생성
            string internalRoomName = "Room_" + Random.Range(1000, 9999);

            Photon.Realtime.RoomOptions options = new Photon.Realtime.RoomOptions {
                MaxPlayers = (byte)_maxPlayers,
                IsVisible = true,
                IsOpen = true,
                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable {
                { "RoomName", roomName },
                { "Password", password },
                { "Map", "Map1" },
            },
                CustomRoomPropertiesForLobby = new[] { "RoomName", "Password", "Map" }
            };

            Photon.Pun.PhotonNetwork.CreateRoom(internalRoomName, options);
        }
    }

    #endregion // private funcs
}
