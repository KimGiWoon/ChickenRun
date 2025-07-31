using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class StartPanelController_Map2 : MonoBehaviourPun
{
    [Header("Start Panel Reference")]
    [SerializeField] GameObject _startPanel;
    [SerializeField] TMP_Text _countText;
    
    private Coroutine _panelRoutine;

    private void Start()
    {
        GameManager_Map2.Instance.OnReadyGame += ReadyGame;
    }
    
    private void ReadyGame()
    {
        photonView.RPC(nameof(StartGameRoutine), RpcTarget.AllViaServer);
    }
    
    // 스타트 코루틴
    [PunRPC]
    private void StartGameRoutine()
    {
        if(_panelRoutine == null)
        {
            _panelRoutine = StartCoroutine(StartPanelCoroutine());
        }
    }

    // 스타트 패널 UI 코루틴
    private IEnumerator StartPanelCoroutine()
    {
        WaitForSeconds time = new WaitForSeconds(1f);
        int count = 3;

        yield return new WaitForSeconds(2f);
        _startPanel.SetActive(true);

        while (true)
        {
            if (count == 0)
            {
                AudioManager_Map2.Instance.PlaySFX(AudioManager_Map2.Sfxs.SFX_Start);
                _countText.text = "시작!";
            }
            else
            {
                AudioManager_Map2.Instance.PlaySFX(AudioManager_Map2.Sfxs.SFX_Count);
                _countText.text = count.ToString();                
            }

            yield return time;
            count--;

            if(count < 0)
            {
                AudioManager_Map2.Instance.PlayBGM(AudioManager_Map2.Bgms.BGM_InGame);
                _startPanel.SetActive(false);
                // _wall.SetActive(false);
                GameManager_Map2.Instance.StartGame();
                _panelRoutine = null;
                break;
            }
        }
    }
}
