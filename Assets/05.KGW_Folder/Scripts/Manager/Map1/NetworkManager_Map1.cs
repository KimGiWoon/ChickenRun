using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;

public class NetworkManager_Map1 : MonoBehaviourPunCallbacks
{
    [Header("Manager Reference")]
    [SerializeField] UIManager_Map1 _UIManager;
    [SerializeField] GameManager_Map1 _gameManager;

    public bool _isStart = false;

    private void Start()
    {
        // 서버에 연결이 되어 있지 않으면 서버 접속
        if (!PhotonNetwork.IsConnected)
        {
            UnityEngine.Debug.Log("서버에 미연결로 서버 접속");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            UnityEngine.Debug.Log("입장 완료");
    
            // 플레이어 생성
            PlayerSpawn();

            // 방에 들어온 플레이어 체크
            if (PhotonNetwork.IsMasterClient)
            {
                CheckRoomPlayer();
            }
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

        // 플레이어 생성
        PlayerSpawn();

        // 방에 들어온 플레이어 체크
        if (PhotonNetwork.IsMasterClient)
        {
            CheckRoomPlayer();
        }
    }

    // 플레이어 생성
    private void PlayerSpawn()
    {
        Vector2 spawnPos = new Vector2(0, 0);
        PhotonNetwork.Instantiate($"Player_Map1", spawnPos, Quaternion.identity);
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

        UnityEngine.Debug.Log($"입장 플레이어 : {currentPlayer}/{maxPlayer}");

        // 현재 인원 전달
        if (PhotonNetwork.IsMasterClient)
        {
            _gameManager._totalPlayerCount = currentPlayer;
        }

        if (currentPlayer >= maxPlayer)
        {
            UnityEngine.Debug.Log("모든 플레이어 입장 완료");
            // 게임 시작
            photonView.RPC(nameof(StartGame), RpcTarget.AllViaServer);
        }
    }

    // 게임 스타트
    [PunRPC]
    private void StartGame()
    {
        _isStart = true;
        _UIManager.photonView.RPC(nameof(_UIManager.StartGameRoutine), RpcTarget.AllViaServer);
    }

    // 플레이어가 방에 들어오면 체크
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UnityEngine.Debug.Log($"Player_{newPlayer.NickName} 입장완료");

        if(PhotonNetwork.IsMasterClient)
        {
            CheckRoomPlayer();
        }
    }
}
