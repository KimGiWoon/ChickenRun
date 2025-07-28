using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class InGameNetworkManager : MonoBehaviourPunCallbacks
{
    static InGameNetworkManager instance;
    public static InGameNetworkManager Instance;

    [SerializeField] Stage1UIManager _UIManager;
    public bool _isStart = false;

    private void Awake()
    {
        // 게임매니저 생성
        CreateNetworkManager();
    }

    private void Start()
    {
        // 서버 접속
        PhotonNetwork.ConnectUsingSettings();
    }

    // 게임매니저가 생성한게 있으면 생성하지 않고 중복으로 생성 시 삭제
    public void CreateNetworkManager()
    {
        if (instance == null)
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
        PlayerSpawn();
    }

    // 플레이어 생성
    private void PlayerSpawn()
    {
        Vector2 spawnPos = new Vector2(0, 0);
        PhotonNetwork.Instantiate($"BasicPlayer", spawnPos, Quaternion.identity);
    }

    // 입장 플레이어 체크
    private void CheckRoomPlayer()
    {
        if (_isStart)
        {
            return;
        }

        // 입장한 플레이어
        int currentPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        // 방에 입장 가능한 Max 플레이어
        int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
        int maxTest = 2;

        UnityEngine.Debug.Log($"입장 플레이어 : {currentPlayer}/{maxTest}");

        if(currentPlayer >= maxTest)
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UnityEngine.Debug.Log($"Player_{newPlayer.NickName} 입장완료");

        if(PhotonNetwork.IsMasterClient)
        {
            CheckRoomPlayer();
        }
    }
}
