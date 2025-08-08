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

        PhotonNetwork.AutomaticallySyncScene = true;
        
        if (_defeatRoutine == null)
        {
            if (GameManager_Map2.Instance.IsLose)
            {
                PhotonNetwork.AutomaticallySyncScene = true;
                if (PhotonNetwork.IsMasterClient)
                {
                    _defeatRoutine = StartCoroutine(DefeatPanelOpen());
                }
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
        
        PhotonNetwork.LoadLevel("MainScene");
    }

    // 게임 탈주 표시 코루틴
    private IEnumerator ExitDefeatPanelOpen()
    {
        gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Defeat);

        yield return _time;

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
