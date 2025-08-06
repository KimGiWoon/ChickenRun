using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

namespace Kst
{
    public class Map3_NetworkManager : MonoBehaviourPunCallbacks
    {
        public bool _isStart = false;
        [Header("Managers")]
        [SerializeField] GameManager_Map3 _gameManager;
        [SerializeField] UIManager_Map3 _UIManager;
        [SerializeField] PlateSpawner _plateSpawner;
        
        [Header("Player")]
        [SerializeField] private Transform spawnPoint; // 플레이어 생성 위치
        [SerializeField] private string playerPrefabName = "Map3_Player";
        [SerializeField] private Map3BtnUI _btnUI;

        //Camera
        [SerializeField] private CinemachineVirtualCamera _virtualCam;

        /// <summary>
        /// 씬 이동 동기화 해제
        /// 방장이 방을 떠나도 게임은 진행할 수 있도록.
        /// </summary>
        void Awake() => PhotonNetwork.AutomaticallySyncScene = false;

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

                // 플레이어 생성
                PlayerSpawn();

                // 방에 들어온 플레이어 체크
                if (PhotonNetwork.IsMasterClient)
                    CheckRoomPlayer();
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Photon Master Server");
            PhotonNetwork.JoinRandomOrCreateRoom(); // 먼저 로비 입장
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");

            PlayerSpawn();

            if (PhotonNetwork.IsMasterClient)
                CheckRoomPlayer();
        }

        // 플레이어 생성
        private void PlayerSpawn()
        {
            // 커스텀 속성에서 스킨 이름 가져오기
            string skinName = "Default";
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Skin", out object skinObj))
                skinName = skinObj.ToString();

            GameObject go = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.position, Quaternion.identity, 0, new object[] { skinName });
            if (go.TryGetComponent(out PhotonView pv) && pv.IsMine)
            {
                Map3_PlayerController player = go.GetComponent<Map3_PlayerController>();
                //버튼 설정
                _btnUI.Init(player);

                //카메라 설정
                _virtualCam.Follow = go.transform;
                _virtualCam.LookAt = go.transform;
            }
        }

        // 입장 플레이어 체크
        private void CheckRoomPlayer()
        {
            if (_isStart) return;

            // 방에 입장한 플레이어
            int currentPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
            // 방에 입장 가능한 Max 플레이어
            int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;

            Debug.Log($"입장 플레이어 : {currentPlayer}/{maxPlayer}");

            if (PhotonNetwork.IsMasterClient)
                _gameManager._totalPlayerCount = currentPlayer;

            if (currentPlayer >= maxPlayer)
            {
                Debug.Log("모든 플레이어 입장 완료");

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
            Debug.Log($"Player_{newPlayer.NickName} 입장완료");

            if (PhotonNetwork.IsMasterClient)
                CheckRoomPlayer();
        }

        //마스터 클라이언트 변경시
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer == newMasterClient)
            {
                Debug.Log("마스터 변경 및 권한 양도");
                GameManager_Map3.Instance.PlateSpawnerSys.StopSpawn();
                GameManager_Map3.Instance.PlateSpawnerSys.StartSpawn();
            }
        }
    }
}