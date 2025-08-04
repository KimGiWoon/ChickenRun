using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DefeatUIController_Map4 : MonoBehaviour
{
    [Header("Clear UI Reference")]
    [SerializeField] GameObject _defeatPanel;
    [SerializeField] UIManager_Map4 _UIManager;
    [SerializeField] float _openTime = 2f;

    Coroutine _defeatRoitine;
    WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);

        if (_defeatRoitine == null)
        {
            if (_UIManager._isExit)
            {
                // 탈주 코루틴 스타트
                _defeatRoitine = StartCoroutine(ExitDefeatPanelOpen());
            }
            else
            {
                // 실패 코루틴 스타트
                _defeatRoitine = StartCoroutine(DefeatPanelOpen());
            }
        }
        else
        {
            _defeatRoitine = null;
        }
    }

    // 게임 실패 패널 표시 코루틴
    private IEnumerator DefeatPanelOpen()
    {
        _defeatPanel.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);

        yield return _time;

        SoundManager.Instance.StopSFX();

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    // 게임 탈주 표시 코루틴
    private IEnumerator ExitDefeatPanelOpen()
    {
        _defeatPanel.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);

        yield return _time;

        SoundManager.Instance.StopSFX();

        // 씬 이동
        PhotonNetwork.LeaveRoom();
    }

    private void OnDestroy()
    {
        StopCoroutine(_defeatRoitine);
    }
}
