using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Kst
{
    public class Map3_NetworkManager : MonoBehaviourPunCallbacks
    {
        //스폰
        [SerializeField] private string playerPrefabName = "Map3_Player";
        [SerializeField] private Map3BtnUI _btnUI;
        [SerializeField] private Transform spawnPoint; // 플레이어 생성 위치

        void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server");
            PhotonNetwork.JoinLobby(); // 먼저 로비 입장
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("Joined Lobby");
            PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");

            GameObject go = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, Quaternion.identity);
            if (go.TryGetComponent(out PhotonView pv) && pv.IsMine)
            {
                Map3_PlayerController player = go.GetComponent<Map3_PlayerController>();
                _btnUI.Init(player);
                
            }
        }
    }
}