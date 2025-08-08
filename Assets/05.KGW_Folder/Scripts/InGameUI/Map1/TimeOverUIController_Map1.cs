using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TimeOverUIController_Map1 : MonoBehaviour
{
    [Header("Clear UI Reference")]
    [SerializeField] GameObject _timeOverPanel;
    [SerializeField] float _openTime = 2f;

    Coroutine _timeOverRoitine;
    WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);
    }

    private void Start()
    {
        // 타임 오버 사운드 재생
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);
        Debug.Log("타임오버 사운드 재생");

        // 클리어 코루틴 스타트
        if (_timeOverRoitine == null)
        {
            // 타임오버 코루틴 스타트
            _timeOverRoitine = StartCoroutine(TimeOverPanelOpen());
        }
        else
        {
            _timeOverRoitine = null;
        }
    }

    // 게임 클리어 패널 표시 코루틴
    private IEnumerator TimeOverPanelOpen()
    {
        _timeOverPanel.SetActive(true);

        yield return _time;

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    private void OnDestroy()
    {
        StopCoroutine(_timeOverRoitine);
        _timeOverRoitine = null;
    }
}
