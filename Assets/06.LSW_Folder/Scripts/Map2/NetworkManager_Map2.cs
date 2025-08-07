using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager_Map2 : MonoBehaviourPunCallbacks
{
    private int _currentPlayer;
    private bool _isStarted;

    private void Awake()
    {
        // 서버 접속
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
     
        SoundManager.Instance.StopBGM();
        
        PhotonNetwork.AutomaticallySyncScene = false;
        PlayerSpawn();
        photonView.RPC(nameof(NotifyPlayer), RpcTarget.MasterClient);
        if (!_isStarted)
        {
            StartCoroutine(DelayStart());
        }
    }

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(3f);
        if (PhotonNetwork.IsMasterClient && _currentPlayer <= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            photonView.RPC(nameof(ReadyGame), RpcTarget.AllViaServer);
        }
    }
    
    // 마스터 서버 접속
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    // 인게임 진입 시 호출되는 이벤트 
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = $"Player{PhotonNetwork.LocalPlayer.ActorNumber}";
        string nickname = PhotonNetwork.LocalPlayer.NickName;

        Hashtable table = new Hashtable { { "Nickname", nickname } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(table);

        PlayerSpawn();
        photonView.RPC(nameof(NotifyPlayer), RpcTarget.MasterClient);
    }

    public override void OnLeftRoom()
    {
        //SceneManager.LoadScene("Room")
        Debug.Log("게임을 나갑니다");
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string team = otherPlayer.CustomProperties["Color"] as string;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Color", out var myTeamObj))
        {
            string myTeam = myTeamObj.ToString();
            if (myTeam == team)
            {
                // todo 팝업을 띄워도 될 듯? (3초 뒤에 나가집니다)
                PhotonNetwork.LoadLevel("MainScene");
            }
        }
    }
    
    // 플레이어 생성
    private void PlayerSpawn()
    {
        Vector2 spawnPos = new Vector2(0, 0);
        
        
        // 커스텀 속성에서 스킨 이름 가져오기
        string skinName = "Default";
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Skin", out object skinObj))
        {
            skinName = skinObj.ToString();
        }

        // 플레이어 생성 시 스킨 데이터 전달
        PhotonNetwork.Instantiate("Player_Map2", spawnPos, Quaternion.identity, 0, new object[] { skinName });
    }

    [PunRPC]
    private void NotifyPlayer()
    {
        _currentPlayer++;
        if (PhotonNetwork.IsMasterClient && _currentPlayer >= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            photonView.RPC(nameof(ReadyGame), RpcTarget.AllViaServer);
        }
    }
    
    [PunRPC]
    private void DelayedPlayer()
    {
        _currentPlayer++;
        if (PhotonNetwork.IsMasterClient && _currentPlayer <= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            photonView.RPC(nameof(ReadyGame), RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    private void ReadyGame()
    {
        Debug.Log("게임 준비 완료");
        _isStarted = true;
        GameManager_Map2.Instance.ReadyGame();
    }
}
