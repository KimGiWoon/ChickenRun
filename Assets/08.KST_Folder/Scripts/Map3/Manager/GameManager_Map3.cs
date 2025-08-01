using System;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class GameManager_Map3 : MonoBehaviourPun
    {
        private static GameManager_Map3 _instance;
        public static GameManager_Map3 Instance { get { return _instance; } set { _instance = value; } }

        [SerializeField] public UIManager_Map3 _gameUIManager;
        [SerializeField] public float _GamePlayTime;
        public Stopwatch _stopwatch;
        public string _currentMapType;
        public float _totalPlayTime;
        int _totalEggCount = 0;
        public int _totalPlayerCount;
        Map3Data _data;

        //이벤트
        public event Action<int> OnEggCountChange;
        public event Action<Map3Data> OnEndGame;


        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);

            _stopwatch = new Stopwatch();
        }

        void Update()
        {
            PlayTimeOverCheck();
        }

        private void PlayTimeOverCheck()
        {
            if (_totalPlayTime > _GamePlayTime)
            {
                GamePlayTimeOver();
            }
        }

        // 게임 시간 초과
        public void GamePlayTimeOver()
        {
            _stopwatch.Stop();
            _data.EggCount = _totalEggCount;
            OnEndGame?.Invoke(_data);
            UnityEngine.Debug.Log("게임 플레이 시간이 지났습니다.");
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }

        // 현재의 방을 나가기
        [PunRPC]
        public void GameClearLeaveRoom()
        {
            UnityEngine.Debug.Log("모든 플레이어가 방을 나갑니다.");
            _stopwatch?.Reset();

            // 현재의 방을 나가기
            //PhotonNetwork.LeaveRoom();

            // 로비 씬이 있으면 추가해서 씬 이동
            PhotonNetwork.LoadLevel("MainScene");
        }

        // 달걀 획득
        public void GetEgg(int eggCount)
        {
            _totalEggCount += eggCount;
            OnEggCountChange?.Invoke(_totalEggCount);   // 이벤트 호출
        }

        // 스탑워치 시작
        public void StartStopWatch()
        {
            // 스탑워치 리셋
            if (_stopwatch != null)
            {
                _stopwatch.Reset();
            }

            // 스탑워치 시작
            _stopwatch.Start();
        }

        // 스탑워치 정지
        public void StopStopWatch()
        {
            _stopwatch.Stop();
            _data.Record = _stopwatch.ElapsedMilliseconds;
        }

        // 플레이 타임 UI 업데이트
        public string PlayTimeUpdate()
        {
            float arrivalTime = (float)_stopwatch.Elapsed.TotalSeconds;
            int minuteTime = (int)arrivalTime / 60;
            float secondTime = arrivalTime % 60;

            // 플레이 시간 저장
            _totalPlayTime = arrivalTime;

            return string.Format($"{minuteTime:D2}:{secondTime:00.00}");
        }


    }
}
