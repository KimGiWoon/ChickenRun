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

    #endregion // private fields





    #region mono funcs

    private void Start()
    {
        _backButton.onClick.AddListener(() => _onBack?.Invoke());
    }

    #endregion // mono funcs





    #region public funcs

    public void Initialize(System.Action onBack)
    {
        _onBack = onBack;
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
        Debug.Log($"방 목록을 새로고침합니다. 방 개수: {rooms.Count}");
        for (int i = 0; i < rooms.Count; i++) {
            if (i >= _roomButtons.Count) {
                RoomButton newBtn = Instantiate(_roomButtonPrefab, _roomListRoot);
                _roomButtons.Add(newBtn);
            }

            _roomButtons[i].gameObject.SetActive(true);
            _roomButtons[i].Initialize(rooms[i]);
        }

        for (int i = rooms.Count; i < _roomButtons.Count; i++) {
            _roomButtons[i].gameObject.SetActive(false);
        }
    }

    #endregion // private funcs
}
