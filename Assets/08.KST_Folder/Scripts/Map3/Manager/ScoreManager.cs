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

        public event Action<int> OnScoreChanged;
        public event Action<int> OnEggChanged;

        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
        }

        public void AddScroe(int amount)
        {
            _score += amount;
            Debug.Log($" 점수 획득 {amount}");
            OnScoreChanged?.Invoke(_score);
        }

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

        [PunRPC]
        public void AddScore(int actorNumber, int score)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                AddScroe(score);
            }
        }

        [PunRPC]
        public void MinusScore(int actorNumber, int score)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                MinusScore(score);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eggAmount">획득 재화량(실제 데이터베이스에 들어가는 양</param>
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