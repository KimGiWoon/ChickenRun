using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClearUIController_Map2 : MonoBehaviour
{
    [SerializeField] float _openTime = 2f;

    private Coroutine _clearRoutine;
    private WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);
        _clearRoutine = StartCoroutine(ClearPanelOpen());
    }

    // 게임 클리어 패널 표시 코루틴
    private IEnumerator ClearPanelOpen()
    {
        gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Clear);

        yield return _time;

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    private void OnDestroy()
    {
        if (_clearRoutine != null)
        {
            StopCoroutine(_clearRoutine);
            _clearRoutine = null;
        }
    }
}
