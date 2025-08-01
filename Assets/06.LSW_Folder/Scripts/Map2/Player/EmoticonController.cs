using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmoticonController : MonoBehaviourPun
{
    [SerializeField] private Sprite[] _emoticonSprite;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private Image _emoticonImage;
    [SerializeField] private TextMeshProUGUI _playerNickname;
    
    private EmoticonUI _emoticonUI;
    private Coroutine _emoticonRoutine;
    private float _emoticonTime = 3f;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            _emoticonUI = FindObjectOfType<EmoticonUI>();

            _emoticonUI.Initialize(
                onSmileBtn: () => OnEmoticon(0),
                onQuizBtn: () => OnEmoticon(1),
                onSurpriseBtn: () => OnEmoticon(2),
                onAngryBtn: () => OnEmoticon(3),
                onLoveBtn: () => OnEmoticon(4),
                onWeepBtn: () => OnEmoticon(5));
        }
    }

    private void Start()
    {
        if (photonView.Owner.CustomProperties.TryGetValue("Nickname", out object nickname))
        {
            _playerNickname.text = nickname?.ToString();
        }
        if (photonView.Owner.CustomProperties.TryGetValue("Color", out object colors))
        {
            string colorHex = colors.ToString();

            // RGBA 타입을 Color로 변환
            if (ColorUtility.TryParseHtmlString(colorHex, out Color nicknameColor))
            {
                _playerNickname.color = nicknameColor;
            }
        }
    }
    
    private void OnEmoticon(int index)
    {
        _speechBubble.SetActive(true);
        _emoticonImage.sprite = _emoticonSprite[index];

        // 실행중인 코루틴 무시하고 코루틴 실행
        if (_emoticonRoutine != null)
        {
            StopCoroutine( _emoticonRoutine );
        }
        
        // 이모티콘 표시 시간 코루틴 시작
        _emoticonRoutine = StartCoroutine(EmoticonPlayTimeCoroutine());

        photonView.RPC(nameof(EmoticonPlay), RpcTarget.Others, index);
    }
    
    // 이모티콘 표시 코루틴
    private IEnumerator EmoticonPlayTimeCoroutine()
    {
        yield return new WaitForSeconds(_emoticonTime);
        _speechBubble.SetActive (false);
    }

    // 자신을 제외한 플레이어에게 이모티콘 표시
    [PunRPC]
    public void EmoticonPlay(int index)
    {        
        // 이모티콘 반투명하게 설정
        Color imageColor = _emoticonImage.color;
        imageColor.a = 0.5f;
        _emoticonImage.color = imageColor;

        // 말풍선 반투명하게 설정
        Image bubbleImage = _speechBubble.GetComponentInChildren<Image>();
        Color bubbleColor = bubbleImage.color;
        bubbleColor.a = 0.5f;
        bubbleImage.color = bubbleColor;

        // 이모티콘 설정 후 생성
        _emoticonImage.sprite = _emoticonSprite[index];
        _speechBubble.SetActive(true);

        // 실행중인 코루틴 무시하고 코루틴 실행
        if (_emoticonRoutine != null)
        {
            StopCoroutine( _emoticonRoutine );
        }
        // 이모티콘 표시 시간 코루틴 시작
        _emoticonRoutine = StartCoroutine(EmoticonPlayTimeCoroutine());
    }
}
