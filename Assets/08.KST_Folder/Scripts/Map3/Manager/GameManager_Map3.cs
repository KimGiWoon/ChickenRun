using System;
using System.Collections;
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
        public PlateSpawner PlateSpawnerSys;
        public Stopwatch _stopwatch;
        public string _currentMapType;
        public float _totalPlayTime;
        public int _totalPlayerCount;
        Map3Data _data;
        [SerializeField] private Map3_NetworkManager _networkManager;

        //이벤트
        public event Action<Map3Data> OnEndGame;

        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);

            _stopwatch = new Stopwatch();
            _data = new("Map3Record");
        }
        void OnEnable()
        {
            _gameUIManager.OnGameStart += StartToSpawn;
            StartCoroutine(DelaySubscribe());
        }
        void OnDisable()
        {
            _gameUIManager.OnGameStart -= StartToSpawn;
            ScoreManager.Instance.OnEggChanged -= GetEgg;
            ScoreManager.Instance.OnScoreChanged -= GetScore;
        }

        IEnumerator DelaySubscribe()
        {
            //scoreManager가 생성되기 전까지는 대기
            yield return new WaitUntil(() => ScoreManager.Instance != null);
            ScoreManager.Instance.OnEggChanged += GetEgg;
            ScoreManager.Instance.OnScoreChanged += GetScore;
        }

        void Update() => PlayTimeOverCheck();

        void StartToSpawn() => PlateSpawnerSys.StartSpawn();

        void GetScore(int score)
        {
            _data.Score = score;
            UnityEngine.Debug.Log($"획득 점수 : {score}");
            UnityEngine.Debug.Log($"데이터에 저장된 점수 : {_data.Score}");
        }

        void GetEgg(int egg)
        {
            _data.EggCount = egg;
            UnityEngine.Debug.Log($"획득 재화 : {egg}");
            UnityEngine.Debug.Log($"데이터에 저장된 점수 : {_data.EggCount}");
        }

        private void PlayTimeOverCheck()
        {
            if (_totalPlayTime > _GamePlayTime)
            {
                GamePlayTimeOver();
            }
        }

        // 게임 시간 초과(게임 종료)
        public void GamePlayTimeOver()
        {
            _stopwatch.Stop();
            OnEndGame?.Invoke(_data);
            UnityEngine.Debug.Log($"스코어 : {_data.Score}");
            UnityEngine.Debug.Log($"계란 : {_data.EggCount}");
            UnityEngine.Debug.Log("게임 플레이 시간이 지났습니다.");

            PlateSpawnerSys.StopSpawn();
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }

        // 현재의 방을 나가기
        [PunRPC]
        public void GameClearLeaveRoom()
        {
            UnityEngine.Debug.Log("모든 플레이어가 방을 나갑니다.");

            _networkManager._isStart = false;
            SoundManager.Instance.StopBGM();

            PhotonNetwork.LeaveRoom();

            PhotonNetwork.LoadLevel("MainScene");
        }

        // 스탑워치 시작
        public void StartStopWatch()
        {
            // 스탑워치 리셋
            if (_stopwatch != null)
                _stopwatch.Reset();

            // 스탑워치 시작
            _stopwatch.Start();
        }

        // 스탑워치 정지
        public void StopStopWatch() => _stopwatch.Stop();

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
