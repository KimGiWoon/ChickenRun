using System;
using System.Collections;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class GameManager_Map3 : MonoBehaviourPun
    {
        [Header("Mangers")]
        private static GameManager_Map3 _instance;
        public static GameManager_Map3 Instance { get { return _instance; } set { _instance = value; } }
        [SerializeField] private Map3_NetworkManager _networkManager;
        public UIManager_Map3 _gameUIManager;
        public PlateSpawner PlateSpawnerSys;
        public EffectPoolManager _effectPoolManager;

        [Header("Playtimes")]
        [SerializeField] public float _GamePlayTime;
        public Stopwatch _stopwatch;
        public float _totalPlayTime;

        [Header("Datas")]
        public string _currentMapType;
        public int _totalPlayerCount;
        public MapData _data;

        [Header("UI")]
        [SerializeField] GameObject _gameOverPanel;

        //Events
        public event Action OnGameEnd;

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

        /// <summary>
        /// 초기화 순서로 인해 scoreManager가 생성되지 않았을 경우 대기 후 구독
        /// </summary>
        IEnumerator DelaySubscribe()
        {
            //scoreManager가 생성되기 전까지는 대기
            yield return new WaitUntil(() => ScoreManager.Instance != null);
            ScoreManager.Instance.OnEggChanged += GetEgg;
            ScoreManager.Instance.OnScoreChanged += GetScore;
        }

        void Update() => PlayTimeOverCheck();

        //플레이트 스폰 시작
        void StartToSpawn() => PlateSpawnerSys.StartSpawn();

        /// <summary>
        /// 획득한 점수 맵 데이터에 저장
        /// </summary>
        /// <param name="score">획득 점수</param>
        void GetScore(int score)
        {
            _data.Record = score;
            UnityEngine.Debug.Log($"획득 점수 : {score}");
            UnityEngine.Debug.Log($"데이터에 저장된 점수 : {_data.Record}");
        }

        /// <summary>
        /// 획득한 에그(재화) 맵 데이터에 저장
        /// </summary>
        /// <param name="egg">획득 재화</param>
        void GetEgg(int egg)
        {
            _data.EggCount = egg;
            UnityEngine.Debug.Log($"획득 재화 : {egg}");
            UnityEngine.Debug.Log($"데이터에 저장된 점수 : {_data.EggCount}");
        }

        /// <summary>
        /// 게임 종료 시간 체크
        /// </summary>
        private void PlayTimeOverCheck()
        {
            if (_totalPlayTime > _GamePlayTime)
                GamePlayTimeOver();
        }
        

        /// <summary>
        /// 게임 시간 초과(게임 종료)시 
        /// </summary>
        public void GamePlayTimeOver()
        {
            //스탑워치 정지
            _stopwatch.Stop();

            //TODO <김승태> : 데이터베이스에 저장하는 코드
            // Database_RecordManager.Instance.SaveUserMap3Record(_data);

            //게임 종료 이벤트 호출
            OnGameEnd?.Invoke();

            //플레이트 스폰 종료
            PlateSpawnerSys.StopSpawn();

            //서버에서 모든 플레이어에게 RPC 호출
            photonView.RPC(nameof(GameClearLeaveRoom), RpcTarget.AllViaServer);
        }

        // 현재의 방을 나가기
        [PunRPC]
        public void GameClearLeaveRoom()
        {
            //초기화
            _networkManager._isStart = false;
            SoundManager.Instance.StopBGM();
            _gameUIManager.ClearPlayerReference();
            
            //게임 종료 패널 활성화
            _gameOverPanel.SetActive(true);
        }

        /// <summary>
        /// 스탑워치 시작
        /// </summary>
        public void StartStopWatch()
        {
            // 스탑워치 리셋
            if (_stopwatch != null)
                _stopwatch.Reset();

            // 스탑워치 시작
            _stopwatch.Start();
        }

        /// <summary>
        /// 스탑워치 정지
        /// </summary>
        public void StopStopWatch() => _stopwatch.Stop();

        /// <summary>
        /// 플레이 타임 UI 업데이트
        /// </summary>
        /// <returns>시간을 문자열로 변환 후 반환</returns>
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
