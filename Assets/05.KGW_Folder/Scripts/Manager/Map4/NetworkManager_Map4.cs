using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager_Map4 : MonoBehaviourPunCallbacks
{
    [Header("Map4 UI Manager Reference")]
    [SerializeField] UIManager_Map4 _UIManager;

    public bool _isStart = false;

    static NetworkManager_Map4 instance;

    public static NetworkManager_Map4 Instance
    {
        get
        {
            if(instance == null)    // 네트워크 매니저가 하이어라키창에 없으면 게임매니저 생성
            {
                GameObject gameObject = new GameObject("NetworkManager_Map4");
                instance = gameObject.GetComponent<NetworkManager_Map4>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        // 네트워크 매니저 생성
        CreateNetworkManager();
    }

    private void Start()
    {
        // 서버에 연결이 되어 있지 않으면 서버 접속
        if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.Debug.Log("서버에 미연결로 서버 접속");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // 네트워크 매니저가 생성한게 있으면 생성하지 않고 중복으로 생성 시 삭제
    public void CreateNetworkManager()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);   
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 마스터 서버 접속
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    // 방 입장
    public override void OnJoinedRoom()
    {
        UnityEngine.Debug.Log("입장 완료");
        // TODO : 플레이어 닉네임 확인용
        PhotonNetwork.LocalPlayer.NickName = $"Player{PhotonNetwork.LocalPlayer.ActorNumber}";

        // 닉네임을 커스텀 프로퍼티로 저장
        string nickname = PhotonNetwork.LocalPlayer.NickName;
        Hashtable hashtable = new Hashtable { { "Nickname", nickname } };

        // 닉네임 정보를 포톤서버에 업로드
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

        PlayerSpawn();
    }

    // 방을 나가기
    public override void OnLeftRoom()
    {
        UnityEngine.Debug.Log("방을 나감");
        //SceneManager.LoadScene("Room")
    }

    // 플레이어 생성
    private void PlayerSpawn()
    {
        Vector2 spawnPos = new Vector2(0, -4f);
        PhotonNetwork.Instantiate($"Player_Map4", spawnPos, Quaternion.identity);
    }

    // 입장 플레이어 체크
    private void CheckRoomPlayer()
    {
        if (_isStart)
        {
            return;
        }

        // 방에 입장한 플레이어
        int currentPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        // 방에 입장 가능한 Max 플레이어
        int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
        int maxTest = 2;

        UnityEngine.Debug.Log($"입장 플레이어 : {currentPlayer}/{maxTest}");

        if (currentPlayer >= maxTest)
        {
            UnityEngine.Debug.Log("모든 플레이어 입장 완료");
            photonView.RPC(nameof(StartGame), RpcTarget.AllViaServer);
        }
    }

    // 게임 스타트
    [PunRPC]
    private void StartGame()
    {
        _UIManager.photonView.RPC(nameof(_UIManager.StartGameRoutine), RpcTarget.AllViaServer);
    }

    // 플레이어가 방에 들어오면 체크
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UnityEngine.Debug.Log($"Player_{newPlayer.NickName} 입장완료");

        if (PhotonNetwork.IsMasterClient)
        {
            CheckRoomPlayer();
        }
    }
}
