using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    #region Singleton

    public static PhotonManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Photon 초기화
        //if (!PhotonNetwork.IsConnected) {
        //    PhotonNetwork.AutomaticallySyncScene = true;
        //    PhotonNetwork.ConnectUsingSettings();
        //    Debug.Log("[Photon] ConnectUsingSettings 호출");
        //}
    }

    #endregion // Singleton





    #region private fields

    private List<RoomInfo> _cachedRoomList = new();
    private Action _onJoinedRoomCallback;
    private Action _onLeftRoomCallback;
    private event Action<List<RoomInfo>> _onRoomListUpdated;
    [SerializeField] private GameObject _kickRelayPrefab; // PhotonKickRelay 프리팹
    private PhotonKickRelay _kickRelayInstance;
    private System.Action _pendingKickedCallback; // 콜백 임시 저장

    #endregion // private fields





    #region public funcs

    public void SetOnJoinedRoomCallback(Action callback)
    {
        _onJoinedRoomCallback = callback;
    }

    public void SetOnLeftRoomCallback(Action callback)
    {
        _onLeftRoomCallback = callback;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void RequestRoomList(Action<List<RoomInfo>> callback)
    {
        if (!PhotonNetwork.InLobby) {
            Debug.LogWarning("[Photon] 아직 로비에 입장하지 않음. JoinLobby 시도.");
            PhotonNetwork.JoinLobby(); // 재시도
            return;
        }

        _onRoomListUpdated += callback;

        if (_cachedRoomList.Count > 0) {
            callback?.Invoke(_cachedRoomList);
        }
    }

    public void CreateRoom(string roomName, string password, int maxPlayers)
    {
        RoomOptions options = new RoomOptions {
            MaxPlayers = (byte)maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable {
                { "Password", password }
            },
            CustomRoomPropertiesForLobby = new[] { "Password" }
        };

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void JoinRandomRoomOrCreate()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    #endregion // public funcs





    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Photon] 마스터 서버 연결됨");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[Photon] 로비 입장 완료");
        SetUserUIDToPhoton();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"[Photon] {roomList.Count}개의 방 정보 업데이트됨");

        _cachedRoomList.Clear();
        foreach (var room in roomList) {
            if (room.RemovedFromList)
                continue;

            _cachedRoomList.Add(room);
        }

        _onRoomListUpdated?.Invoke(_cachedRoomList);
        _onRoomListUpdated = null;
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("[Photon] 방 생성 완료");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] 방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Photon] 방 참가 완료");

        // PhotonKickRelay 프리팹 인스턴스 생성
        GameObject relayObj = PhotonNetwork.Instantiate("PhotonKickRelay", Vector3.zero, Quaternion.identity);
        PhotonKickRelay relay = relayObj.GetComponent<PhotonKickRelay>();
        _kickRelayInstance = relay;

        // 지연 저장된 콜백이 있다면 Relay에 전달
        if (_pendingKickedCallback != null) {
            _kickRelayInstance.SetOnKickedCallback(_pendingKickedCallback);
        }

        // 방 참가 콜백 실행
        _onJoinedRoomCallback?.Invoke();
        _onJoinedRoomCallback = null;
    }

    public void SetOnKickedCallback(Action onkick)
    {
        _pendingKickedCallback = onkick;
        if (_kickRelayInstance != null) {
            _kickRelayInstance.SetOnKickedCallback(onkick);
        }
    }

    public void KickPlayer(Player targetPlayer, string uid)
    {
        if (_kickRelayInstance != null) {
            _kickRelayInstance.KickPlayer(targetPlayer, uid);
        }
        else {
            Debug.LogWarning("[Photon] KickRelay 인스턴스가 없습니다. 방에 참가 후 사용해야 합니다.");
        }
    }

    public void OnKicked()
    {
        _pendingKickedCallback?.Invoke();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Photon] 방 참가 실패: {message}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("[Photon] 빠른 매치 실패 → 새 방 생성");
        string randomRoomName = "Quick_" + UnityEngine.Random.Range(1000, 9999);
        CreateRoom(randomRoomName, "", 8);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[Photon] 방에서 나감");
        _onLeftRoomCallback?.Invoke();
        _onLeftRoomCallback = null;

        if (_kickRelayInstance != null) {
            PhotonNetwork.Destroy(_kickRelayInstance.gameObject);
            _kickRelayInstance = null;
        }
    }

    #endregion // Photon Callbacks





    #region private funcs

    private void SetUserUIDToPhoton()
    {
        if (CYH_FirebaseManager.User != null) {
            string uid = CYH_FirebaseManager.User.UserId;
            string nickname = CYH_FirebaseManager.User.DisplayName;
            string colorHex = ColorUtility.ToHtmlStringRGBA(Color.black);

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable {
            { "UID", uid },
            { "Nickname", nickname },
            {"Color", colorHex }
        };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log($"[Photon] UID 등록 완료: {uid}");
            Debug.Log($"[Photon] 닉네임 등록 완료: {nickname}");
        }
        else {
            Debug.LogWarning("[Photon] Firebase 로그인 정보가 없어 UID 및 닉네임 등록 실패");
        }
    }

    #endregion // private funcs
}
