using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEmoticonController : MonoBehaviourPun
{
    [SerializeField] public GameObject _SpeechBubble;
    [SerializeField] public Image _emoticonImage;
    [SerializeField] Sprite[] _emoticonSprite;
    [SerializeField] float _emoticonTime = 3f;

    Coroutine _emoticonRoutine;

    private void Start()
    {
        // 이모티콘 패널 전달
        EmoticonPanelDeliver();
    }
    
    // 이모티콘 컨트롤러 전달
    private void EmoticonPanelDeliver()
    {       
        // 자신의 이모티콘 컨트롤러 전달
        if (photonView.IsMine)
        {
            Stage1UIManager uiManager = FindObjectOfType<Stage1UIManager>();

            if (uiManager != null)
            {
                uiManager.GetPlayerEmoticonController(gameObject);
            }
        }
    }

    // 이모티콘 플레이
    [PunRPC]
    public void EmoticonPlay(int index)
    {        
        // 이모티콘 반투명하게 설정
        Color imageColor = _emoticonImage.color;
        imageColor.a = 0.5f;
        _emoticonImage.color = imageColor;

        // 말풍선 반투명하게 설정
        Image bubbleImage = _SpeechBubble.GetComponentInChildren<Image>();
        Color bubbleColor = bubbleImage.color;
        bubbleColor.a = 0.5f;
        bubbleImage.color = bubbleColor;

        // 이모티콘 설정 후 생성
        _emoticonImage.sprite = _emoticonSprite[index];
        _SpeechBubble.SetActive(true);

        // 실행중인 코루틴 무시하고 코루틴 실행
        if (_emoticonRoutine != null)
        {
            StopCoroutine( _emoticonRoutine );
        }
        // 이모티콘 표시 시간 코루틴 시작
        _emoticonRoutine = StartCoroutine(EmoticonPlayTimeCoroutine());
    }

    // 이모티콘 표시 코루틴
    private IEnumerator EmoticonPlayTimeCoroutine()
    {
        yield return new WaitForSeconds( _emoticonTime );
        _SpeechBubble.SetActive (false);
    }
}
