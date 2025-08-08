using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClearUIController_Map1 : MonoBehaviour
{
    [Header("Clear UI Reference")]
    [SerializeField] GameObject _clearPanel;
    [SerializeField] float _openTime = 2f;

    Coroutine _clearRoitine;
    WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);
    }

    private void Start()
    {
        // 성공 사운드 재생
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Clear);
        Debug.Log("성공 사운드 재생");

        if (_clearRoitine == null)
        {
            // 클리어 코루틴 스타트
            _clearRoitine = StartCoroutine(ClearPanelOpen());
        }
        else
        {
            _clearRoitine = null;
        }
    }

    // 게임 클리어 패널 표시 코루틴
    private IEnumerator ClearPanelOpen()
    {
        _clearPanel.SetActive(true);

        yield return _time;

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    private void OnDestroy()
    {
        StopCoroutine(_clearRoitine);
        _clearRoitine = null;
    }
}
