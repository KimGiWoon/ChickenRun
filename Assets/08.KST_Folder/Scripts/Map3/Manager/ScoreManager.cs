using UnityEngine;
using System;

namespace Kst
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        private int _score;
        public int Score { get { return _score; } set { _score = value; } }

        public event Action<int> OnScoreChanged;

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

    }
}