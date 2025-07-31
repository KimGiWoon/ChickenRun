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

        public event Action<int> OnScoreChanged;

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
            Debug.Log(" 점수 획득 ");
            OnScoreChanged?.Invoke(_score);
        }

        public void MinusScore(int amount)
        {
            _score -= amount;
            Debug.Log(" 점수 차감 ");
            if (_score < 0) _score = 0;
            OnScoreChanged?.Invoke(_score);
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

        [PunRPC]
        public void GiveEgg(int actorNumber, int eggAmount)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                EggManager.Instance.GainEgg(eggAmount);
            }
        }
        #endregion

    }
}