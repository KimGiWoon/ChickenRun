using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClearUIController_Map1 : MonoBehaviour
{
    [Header("Clear UI Reference")]
    [SerializeField] GameObject _clearPanel;
    [SerializeField] NetworkManager_Map1 _networkManager;
    [SerializeField] float _openTime = 2f;

    Coroutine _clearRoitine;
    WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);
    }

    private void Start()
    {
        // 클리어 코루틴 스타트
        _clearRoitine = StartCoroutine(ClearPanelOpen());
    }

    // 게임 클리어 패널 표시 코루틴
    private IEnumerator ClearPanelOpen()
    {
        _clearPanel.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Clear);

        yield return _time;

        _networkManager._isStart = false;
        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    private void OnDestroy()
    {
        StopCoroutine(_clearRoitine);
    }
}
