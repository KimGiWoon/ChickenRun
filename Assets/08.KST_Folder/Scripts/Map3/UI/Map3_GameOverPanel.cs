using System.Collections;
using Kst;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Map3_GameOverPanel : MonoBehaviour
{
    [Header("Clear UI Reference")]
    [SerializeField] float _openTime = 4f;
    [SerializeField] TMP_Text _timerText;
    [SerializeField] TMP_Text _scoreText;
    [SerializeField] TMP_Text _eggText;

    Coroutine _clearRoitine;

    private void Start()
    {
        // 클리어 코루틴 스타트
        _clearRoitine = StartCoroutine(ClearPanelOpen());
        _scoreText.text = $"점수 : {GameManager_Map3.Instance._data.Score}";
        _eggText.text = $"획득 재화 : {GameManager_Map3.Instance._data.EggCount}";
    }

    // 게임 클리어 패널 표시 코루틴
    private IEnumerator ClearPanelOpen()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Clear);
        float remain = _openTime;
        while (remain > 0)
        {
            _timerText.text = $"{remain:F0}초 후에 게임을 떠납니다.";
            remain -= Time.deltaTime;
            yield return null;
        }


        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    private void OnDestroy()
    {
        StopCoroutine(_clearRoitine);
    }
}
