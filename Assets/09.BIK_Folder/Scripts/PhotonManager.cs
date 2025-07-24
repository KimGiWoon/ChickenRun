using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : Singleton<PhotonManager>, ILobbyCallbacks, IMatchmakingCallbacks
{
    #region private fields

    private List<RoomInfo> _cachedRoomList = new();
    private Action _onJoinedRoomCallback;
    private Action _onLeftRoomCallback;

    #endregion // private fields





    #region mono funcs

    protected override void Awake()
    {
        base.Awake();

        if (!PhotonNetwork.IsConnected) {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
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





    #region public funcs

    /// <summary>
    /// 방에 입장했을 때 호출되는 콜백을 설정합니다.
    /// </summary>
    /// <param name="callback"></param>
    public void SetOnJoinedRoomCallback(Action callback)
    {
        _onJoinedRoomCallback = callback;
    }

    /// <summary>
    /// 방에서 나갔을 때 호출되는 콜백을 설정합니다.
    /// </summary>
    /// <param name="callback"></param>
    public void SetOnLeftRoomCallback(Action callback)
    {
        _onLeftRoomCallback = callback;
    }

    /// <summary>
    /// 방을 나갑니다.
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 방 목록을 불러오고 실행할 콜백을 전달합니다.
    /// </summary>
    /// <param name="callback"></param>
    public void RequestRoomList(Action<List<RoomInfo>> callback)
    {
        callback?.Invoke(_cachedRoomList);
    }

    /// <summary>
    /// 방을 생성합니다.
    /// </summary>
    /// <param name="roomName">방 제목</param>
    /// <param name="password">비밀번호</param>
    /// <param name="maxPlayers">최대 인원 수</param>
    public void CreateRoom(string roomName, string password, int maxPlayers)
    {
        RoomOptions options = new RoomOptions {
            MaxPlayers = (byte)maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = new Hashtable {
                { "Password", password }
            },
            CustomRoomPropertiesForLobby = new[] { "Password" }
        };

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    /// <summary>
    /// 지정된 방에 참가합니다.
    /// </summary>
    /// <param name="roomName"></param>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// 랜덤한 방에 참가하거나, 방이 없으면 새로 생성합니다.
    /// </summary>
    public void JoinRandomRoomOrCreate()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    #endregion // public funcs





    #region Photon callbacks

    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnJoinedLobby()
    {
        Debug.Log("[Photon] 로비 입장 완료");
    }

    public void OnLeftLobby()
    {
        Debug.Log("[Photon] 로비에서 나감");
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        // Do Nothing
    }

    /// <summary>
    /// 로비에 있는 방 목록이 업데이트되었을 때 호출됩니다.
    /// </summary>
    /// <param name="roomList"></param>
    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _cachedRoomList.Clear();

        foreach (var room in roomList) {
            if (room.RemovedFromList || room.PlayerCount >= room.MaxPlayers)
                continue;

            _cachedRoomList.Add(room);
        }
    }

    public void OnCreatedRoom()
    {
        Debug.Log("[Photon] 방 생성 완료");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] 방 생성 실패: {message}");
    }

    public void OnJoinedRoom()
    {
        Debug.Log("[Photon] 방 참가 완료");

        _onJoinedRoomCallback?.Invoke();
        _onJoinedRoomCallback = null;
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] 방 참가 실패: {message}");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("[Photon] 빠른 매치 실패 → 새 방 생성");
        string randomRoomName = "Quick_" + UnityEngine.Random.Range(1000, 9999);
        CreateRoom(randomRoomName, "", 8);
    }

    public void OnLeftRoom()
    {
        _onLeftRoomCallback?.Invoke();
        _onLeftRoomCallback = null;
        Debug.Log("[Photon] 방에서 나감");
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        // Do Nothing
    }

    #endregion // Photon callbacks
}
