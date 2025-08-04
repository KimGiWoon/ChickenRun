using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEmoticonController_Map3 : MonoBehaviourPunCallbacks
{
    [SerializeField] public GameObject _SpeechBubble;
    [SerializeField] public Image _emoticonImage;
    [SerializeField] TMP_Text _nicknameText;
    [SerializeField] Sprite[] _emoticonSprite;
    [SerializeField] float _emoticonTime = 3f;

    Coroutine _emoticonRoutine;

    private void Start()
    {
        // 이모티콘 패널 전달
        EmoticonPanelDeliver();

        // 키값을 가진 프로퍼티가 있는지 확인 후 있으면 아웃 변수에 저장
        if (photonView.Owner.CustomProperties.TryGetValue("Nickname", out object nickname))
        {
            _nicknameText.text = nickname.ToString();
        }

        // 닉네임 컬러가 있는지 확인
        if (photonView.Owner.CustomProperties.TryGetValue("Color", out object colors))
        {
            string colorHex = colors.ToString();

            // RGBA 타입을 Color로 변환
            if (ColorUtility.TryParseHtmlString(colorHex, out Color nicknameColor))
            {
                _nicknameText.color = nicknameColor;
            }
        }
    }

    // 이모티콘 컨트롤러 전달
    private void EmoticonPanelDeliver()
    {
        // 자신의 이모티콘 컨트롤러 전달
        if (photonView.IsMine)
        {
            UIManager_Map3 uiManager = FindObjectOfType<UIManager_Map3>();

            if (uiManager != null)
            {
                uiManager.GetPlayerEmoticonController(gameObject);
            }
        }
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
            StopCoroutine(_emoticonRoutine);
        }
        // 이모티콘 표시 시간 코루틴 시작
        _emoticonRoutine = StartCoroutine(EmoticonPlayTimeCoroutine());

        Debug.Log("{EmoticonPlay} 내부 실행");
    }

    // 이모티콘 표시 코루틴
    private IEnumerator EmoticonPlayTimeCoroutine()
    {
        yield return new WaitForSeconds(_emoticonTime);
        _SpeechBubble.SetActive(false);
    }

    // 플레이어의 커스텀 프로퍼티가 변경 확인
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // 프로퍼티의 소유자가 맞고 키값을 가지고 있는지 체크
        if (photonView.Owner == targetPlayer && changedProps.ContainsKey("Nickname"))
        {
            _nicknameText.text = changedProps["Nickname"].ToString();

            if (changedProps.ContainsKey("Color"))
            {
                string colorHex = changedProps["Color"].ToString();
                if (ColorUtility.TryParseHtmlString(colorHex, out Color parsedColor))
                {
                    _nicknameText.color = parsedColor;
                }
            }
        }
    }
}
