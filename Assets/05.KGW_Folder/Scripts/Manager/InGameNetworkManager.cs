using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class InGameNetworkManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {   
        // 서버 접속
        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버 접속
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    // 방 입장
    public override void OnJoinedRoom()
    {
        Debug.Log("입장 완료");
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


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player_{newPlayer.NickName} 입장완료");
    }
}
