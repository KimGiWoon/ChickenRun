using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Kst
{
    public class Map3_NetworkManager : MonoBehaviourPunCallbacks
    {
        //스폰
        [SerializeField] private string playerPrefabName = "Map3_Player";
        [SerializeField] private Map3BtnUI _btnUI;
        [SerializeField] private Transform spawnPoint; // 플레이어 생성 위치
        [SerializeField] PlateSpawner _plateSpawner;
        [SerializeField] UIManager_Map3 _UIManager;
        private bool _isStart = false;

        void Start()
        {
            // 서버에 연결이 되어 있지 않으면 서버 접속
            if (!PhotonNetwork.IsConnected)
            {
                Debug.Log("서버에 미연결로 서버 접속");
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                Debug.Log("입장 완료");

                // 닉네임을 커스텀 프로퍼티로 저장
                string nickname = PhotonNetwork.LocalPlayer.NickName;
                Hashtable hashtable = new Hashtable { { "Nickname", nickname } };

                // 닉네임 정보를 포톤서버에 업로드
                PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

                // 플레이어 생성
                PlayerSpawn();

                // 방에 들어온 플레이어 체크
                if (PhotonNetwork.IsMasterClient)
                {
                    CheckRoomPlayer();
                }
            }
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

        // 방을 나가기
        public override void OnLeftRoom()
        {
            Debug.Log("방을 나감");
            //SceneManager.LoadScene("Room")
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");

            PhotonNetwork.LocalPlayer.NickName = $"Player{PhotonNetwork.LocalPlayer.ActorNumber}";

            // 닉네임을 커스텀 프로퍼티로 저장
            string nickname = PhotonNetwork.LocalPlayer.NickName;
            Hashtable hashtable = new Hashtable { { "Nickname", nickname } };

            // 닉네임 정보를 포톤서버에 업로드
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

            PlayerSpawn();

            // GameObject go = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, Quaternion.identity);
            // if (go.TryGetComponent(out PhotonView pv) && pv.IsMine)
            // {
            //     Map3_PlayerController player = go.GetComponent<Map3_PlayerController>();
            //     _btnUI.Init(player);
            // }
            // _plateSpawner.StartSpawn();
        }

        // 플레이어 생성
        private void PlayerSpawn()
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, Quaternion.identity);
            if (go.TryGetComponent(out PhotonView pv) && pv.IsMine)
            {
                Map3_PlayerController player = go.GetComponent<Map3_PlayerController>();
                _btnUI.Init(player);
            }
            _plateSpawner.StartSpawn();
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

            Debug.Log($"입장 플레이어 : {currentPlayer}/{maxPlayer}");

            if (currentPlayer >= maxPlayer)
            {
                Debug.Log("모든 플레이어 입장 완료");
                GameManager_Map1.Instance._totalPlayerCount = currentPlayer;
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
            Debug.Log($"Player_{newPlayer.NickName} 입장완료");

            if (PhotonNetwork.IsMasterClient)
            {
                CheckRoomPlayer();
            }
        }


    }
}