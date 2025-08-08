using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayFindRoomPanel : UIBase
{
    #region Serialized fields

    [Header("Room List UI")]
    [SerializeField] private RectTransform _roomListRoot;
    [SerializeField] private RoomButton _roomButtonPrefab;

    [Header("Back Button")]
    [SerializeField] private Button _backButton;

    #endregion // Serialized fields





    #region private fields

    private List<RoomButton> _roomButtons = new();
    private System.Action _onBack;
    private System.Action _onJoin;

    #endregion // private fields





    #region mono funcs

    private void Awake()
    {
        _roomButtons = new List<RoomButton>();

        foreach (Transform child in _roomListRoot) {
            RoomButton btn = child.GetComponent<RoomButton>();
            if (btn != null) {
                btn.gameObject.SetActive(false);
                _roomButtons.Add(btn);
            }
        }
    }

    private void Start()
    {
        _backButton.onClick.AddListener(() => _onBack?.Invoke());
    }

    #endregion // mono funcs





    #region public funcs

    public void Initialize(System.Action onBack, System.Action onJoin)
    {
        _onBack = onBack;
        _onJoin = onJoin;
    }

    public override void RefreshUI()
    {
        // 화면을 새로고침 할때마다 방 목록을 요청합니다.
        Debug.LogWarning("방을 찾는 중입니다.");
        PhotonManager.Instance.RequestRoomList(OnRoomListReceived);
    }

    #endregion // public funcs





    #region private funcs

    /// <summary>
    /// 방 목록을 받아왔을 때 호출되는 콜백 함수입니다.
    /// </summary>
    /// <param name="roomList"></param>
    private void OnRoomListReceived(List<RoomInfo> roomList)
    {
        Debug.Log($"방 목록을 받아왔습니다. 방 개수: {roomList.Count}");
        RefreshRoomList(roomList);
    }

    private void RefreshRoomList(List<RoomInfo> rooms)
    {
        if (this == null || !gameObject.activeInHierarchy || _roomListRoot == null)
            return;

        Debug.Log($"방 목록을 새로고침합니다. 방 개수: {rooms.Count}");

        int i = 0;
        for (; i < rooms.Count && i < _roomButtons.Count; i++) {
            _roomButtons[i].gameObject.SetActive(true);
            _roomButtons[i].Initialize(rooms[i], OnJoinedRoom);
        }

        // 나머지 버튼은 끈다
        for (; i < _roomButtons.Count; i++) {
            _roomButtons[i].gameObject.SetActive(false);
        }
    }

    private void OnJoinedRoom()
    {
        _onJoin?.Invoke();
    }

    #endregion // private funcs
}
