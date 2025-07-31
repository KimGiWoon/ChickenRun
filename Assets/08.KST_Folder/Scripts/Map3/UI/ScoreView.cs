using TMPro;
using UnityEngine;
namespace Kst
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;

        void OnEnable()
        {
            ScoreManager.Instance.OnScoreChanged += RefreshUI;
        }
        void OnDisable()
        {
            ScoreManager.Instance.OnScoreChanged -= RefreshUI;
        }

        void Start()
        {
            RefreshUI(ScoreManager.Instance.Score);
        }

        void RefreshUI(int score)
        {
            _scoreText.text = $"점수 : {score}";
        }
    }

}