using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager_Map2 : MonoBehaviourPunCallbacks
{
    private int _currentPlayer;

    private void Awake()
    {
        // 서버 접속
        PhotonNetwork.ConnectUsingSettings();
    }
    
    private void Start()
    {   
        GameManager_Map2.Instance.OnTimeUp += () => PhotonNetwork.LeaveRoom();
    }

    // 마스터 서버 접속
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    // 인게임 진입 시 호출되는 이벤트 
    public override void OnJoinedRoom()
    {
        Debug.Log("입장 완료");
        // TODO : 플레이어 닉네임 확인용
        PhotonNetwork.LocalPlayer.NickName = $"Player{PhotonNetwork.LocalPlayer.ActorNumber}";
        PlayerSpawn();
        photonView.RPC(nameof(NotifyPlayer), RpcTarget.MasterClient);
    }

    public override void OnLeftRoom()
    {
        //SceneManager.LoadScene("Room")
    }
    
    // 플레이어 생성
    private void PlayerSpawn()
    {
        Vector2 spawnPos = new Vector2(0, 0);
        PhotonNetwork.Instantiate($"Player_Map2", spawnPos, Quaternion.identity);
    }

    [PunRPC]
    private void NotifyPlayer()
    {
        _currentPlayer++;
        if (PhotonNetwork.IsMasterClient && _currentPlayer == 2)
        {
            photonView.RPC(nameof(StartGame), RpcTarget.All);
        }
    }

    [PunRPC]
    private void StartGame()
    {
        Debug.Log("게임 시작");
        GameManager_Map2.Instance.StartGame();
    }
}
