using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DefeatUIController_Map2 : MonoBehaviour
{
    [SerializeField] float _openTime = 2f;

    private Coroutine _defeatRoutine;
    private WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);

        if (_defeatRoutine == null)
        {
            if (GameManager_Map2.Instance.IsLose)
            {
                // 실패 코루틴 스타트
                _defeatRoutine = StartCoroutine(DefeatPanelOpen());
            }
            else
            {
                // 탈주 코루틴 스타트
                _defeatRoutine = StartCoroutine(ExitDefeatPanelOpen());
            }
        }
        else
        {
            _defeatRoutine = null;
        }
    }

    // 게임 실패 패널 표시 코루틴
    private IEnumerator DefeatPanelOpen()
    {
        gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);

        yield return _time;

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    // 게임 탈주 표시 코루틴
    private IEnumerator ExitDefeatPanelOpen()
    {
        gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);

        yield return _time;

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
        PhotonNetwork.LeaveRoom();
    }

    private void OnDestroy()
    {
        if (_defeatRoutine != null)
        {
            StopCoroutine(_defeatRoutine);
            _defeatRoutine = null;
        }
    }
}
