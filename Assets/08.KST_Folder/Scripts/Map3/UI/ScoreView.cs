using System.Collections;
using TMPro;
using UnityEngine;
namespace Kst
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _eggText;

        void OnEnable()
        {
            // ScoreManager.Instance.OnScoreChanged += RefreshUI;

            StartCoroutine(DelaySubscribe());
        }
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

        IEnumerator DelaySubscribe()
        {
            //scoreManager가 생성되기 전까지는 대기
            yield return new WaitUntil(() => ScoreManager.Instance != null);
            ScoreManager.Instance.OnScoreChanged += RefreshScoreUI;
            ScoreManager.Instance.OnEggChanged += RefreshEggUI;
        }

        void RefreshScoreUI(int score)
        {
            _scoreText.text = $"점수 : {score}";
            Debug.Log("UI 초기화 실행");
        }
        void RefreshEggUI(int egg)
        {
            _eggText.text = $"{egg}";
            Debug.Log("UI 초기화 실행");
        }
    }

}