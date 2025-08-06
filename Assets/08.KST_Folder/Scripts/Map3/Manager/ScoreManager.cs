using UnityEngine;
using System;
using Photon.Pun;

namespace Kst
{
    public class ScoreManager : MonoBehaviourPun
    {
        private static ScoreManager _instance;
        public static ScoreManager Instance { get { return _instance; } set { _instance = value; } }
        private int _score;
        public int Score { get { return _score; } set { _score = value; } }
        private int _egg;
        public int Egg { get { return _egg; } set { _egg = value; } }

        //이벤트
        public event Action<int> OnScoreChanged;
        public event Action<int> OnEggChanged;

        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// 점수 추가 로직
        /// 점수 추가 및 이벤트 호출
        /// </summary>
        /// <param name="amount">획득 점수량</param>
        public void AddScroe(int amount)
        {
            _score += amount;
            Debug.Log($" 점수 획득 {amount}");
            OnScoreChanged?.Invoke(_score);
        }

        /// <summary>
        /// 점수 차감 로직
        /// 차감될때 점수가 0보다 이하일 경우 0으로 설정
        /// </summary>
        /// <param name="amount">차감 점수량</param>
        public void MinusScore(int amount)
        {
            _score += amount;
            Debug.Log($" 점수 차감 {amount}");
            if (_score < 0) _score = 0;
            OnScoreChanged?.Invoke(_score);
        }
        public void AddEgg(int amount)
        {
            _egg += amount;
            Debug.Log($" 계란 획득 {amount}");
            OnEggChanged?.Invoke(_egg);
        }


        #region RPC

        /// <summary>
        /// 해당 플레이어에게 일정 점수 부여
        /// </summary>
        /// <param name="actorNumber">해당 플레이어 액터넘버</param>
        /// <param name="score">획득 점수</param>
        [PunRPC]
        public void AddScore(int actorNumber, int score)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
                AddScroe(score);
        }

        /// <summary>
        /// 해당 플레이어에게 일정 점수만큼 차감
        /// </summary>
        /// <param name="actorNumber">해당 플레이어 액터넘버</param>
        /// <param name="score">차감 점수</param>
        [PunRPC]
        public void MinusScore(int actorNumber, int score)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
                MinusScore(score);
        }

        /// <summary>
        /// 재화 획득 시 데이터베이스에 저장
        /// </summary>
        /// <param name="eggAmount">획득 재화량</param>
        [PunRPC]
        public void GiveEgg(int actorNumber, int eggAmount)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                AddEgg(eggAmount);
                EggManager.Instance.GainEgg(eggAmount);
            }
        }
        #endregion

    }
}