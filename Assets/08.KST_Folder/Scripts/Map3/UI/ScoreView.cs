using System.Collections;
using TMPro;
using UnityEngine;
namespace Kst
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _eggText;

        void OnEnable() => StartCoroutine(DelaySubscribe());
        void OnDisable()
        {
            ScoreManager.Instance.OnScoreChanged -= RefreshScoreUI;
            ScoreManager.Instance.OnEggChanged -= RefreshEggUI;
        }

        void Start()
        {
            RefreshScoreUI(ScoreManager.Instance.Score);
            RefreshEggUI(ScoreManager.Instance.Egg);
        }

        /// <summary>
        /// 초기화 순서로 인해 scoreManager가 생성되지 않았을 경우 대기 후 구독
        /// </summary>
        IEnumerator DelaySubscribe()
        {
            //scoreManager가 생성되기 전까지는 대기
            yield return new WaitUntil(() => ScoreManager.Instance != null);
            ScoreManager.Instance.OnScoreChanged += RefreshScoreUI;
            ScoreManager.Instance.OnEggChanged += RefreshEggUI;
        }

        /// <summary>
        /// 점수 UI 초기화 후 재표기
        /// </summary>
        /// <param name="score">획득 점수</param>
        void RefreshScoreUI(int score)
        {
            _scoreText.text = $"점수 : {score}";
            Debug.Log("UI 초기화 실행");
        }

        /// <summary>
        /// 재화 UI 초기화 후 재표기
        /// </summary>
        /// <param name="egg">획득 에그(재화)</param>
        void RefreshEggUI(int egg)
        {
            _eggText.text = $"x {egg}";
            Debug.Log("UI 초기화 실행");
        }
    }

}