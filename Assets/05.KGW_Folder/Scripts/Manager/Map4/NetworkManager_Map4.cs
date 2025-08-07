using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;

public class NetworkManager_Map4 : MonoBehaviourPunCallbacks
{
    [Header("Map4 UI Manager Reference")]
    [SerializeField] UIManager_Map4 _UIManager;
    [SerializeField] GameManager_Map4 _gameManager;

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

            // 로비 BGM 정지
            SoundManager.Instance.StopBGM();

            // 씬이동 관련 동기화 해제
            PhotonNetwork.AutomaticallySyncScene = false;

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
        if (_isStart)
        {
            return;
        }

        Vector2 spawnPos = new Vector2(0, -4f);

        // 커스텀 속성에서 스킨 이름 가져오기
        string skinName = "Default";
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Skin", out object skinObj))
        {
            skinName = skinObj.ToString();
        }

        // 플레이어 생성 시 스킨 데이터 전달
        PhotonNetwork.Instantiate("Player_Map4", spawnPos, Quaternion.identity, 0, new object[] { skinName });
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

        // 현재 인원 전달
        if (PhotonNetwork.IsMasterClient)
        {
            _gameManager._totalPlayerCount = currentPlayer;
        }

        if (currentPlayer >= 1)
        {
            UnityEngine.Debug.Log("모든 플레이어 입장 완료");
            // 게임 시작
            photonView.RPC(nameof(StartGame), RpcTarget.AllViaServer);
        }
    }

    // 방 나가기 콜백
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MainScene");
    }

    // 게임 스타트
    [PunRPC]
    private void StartGame()
    {
        // 플레이어 생성
        PlayerSpawn();

        _isStart = true;
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
