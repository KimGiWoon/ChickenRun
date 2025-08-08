using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DefeatUIController_Map1 : MonoBehaviour
{
    [Header("Clear UI Reference")]
    [SerializeField] GameObject _defeatPanel;
    [SerializeField] UIManager_Map1 _UIManager;
    [SerializeField] float _openTime = 2f;

    Coroutine _defeatRoitine;
    WaitForSeconds _time;

    private void OnEnable()
    {
        _time = new WaitForSeconds(_openTime);        
    }

    private void Start()
    {
        if (_defeatRoitine == null)
        {
            // 실패 사운드 재생
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);
            Debug.Log("실패 사운드 재생");

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

        yield return _time;

        // 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    // 게임 탈주 표시 코루틴
    private IEnumerator ExitDefeatPanelOpen()
    {
        _defeatPanel.SetActive(true);

        yield return _time;

        // 씬 이동
        PhotonNetwork.LeaveRoom();
    }

    private void OnDestroy()
    {
        StopCoroutine(_defeatRoitine);
        _defeatRoitine = null;
    }
}
