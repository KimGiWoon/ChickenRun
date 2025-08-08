using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    #region Serialized fields

    [Header("UI")]
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private GameObject _readyOffIcon;
    [SerializeField] private GameObject _readyOnIcon;

    #endregion // Serialized fields





    #region private fields

    private Player _player;
    private float _pressTime;
    private bool _isHolding;
    private const float KickHoldThreshold = 2f;

    #endregion // private fields





    #region public funcs

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetPlayer(Player player)
    {
        _player = player;

        // 닉네임 설정
        if (player.CustomProperties.TryGetValue("Nickname", out object nicknameObj) && nicknameObj is string nickname) {
            _nicknameText.text = nickname;
        }
        else {
            _nicknameText.text = player.NickName;
        }

        // 닉네임 색
        if (player.CustomProperties.TryGetValue("Color", out object colorObj) && colorObj is string colorStr) {
            if (System.Enum.TryParse(colorStr, out ColorType colorType)) {
                _nicknameText.color = Common.ConvertColorTypeToUnityColor(colorType);
            }
            else {
                _nicknameText.color = Color.white;
            }
        }
        else {
            _nicknameText.color = Color.white;
        }

        // 준비 상태
        bool isReady = false;
        if (player.CustomProperties.TryGetValue("IsReady", out object readyValue) && readyValue is bool b) {
            isReady = b;
        }

        _readyOnIcon.SetActive(isReady);
        _readyOffIcon.SetActive(!isReady);
    }

    #endregion // public funcs





    #region Pointer Events

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHolding = true;
        _pressTime = 0f;
        StartCoroutine(HoldCheckCoroutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pressTime < KickHoldThreshold) {
            OnClick(); // 짧은 클릭 → 정보 보기
        }

        _isHolding = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHolding = false;
    }

    #endregion // Pointer Events





    #region private funcs

    private System.Collections.IEnumerator HoldCheckCoroutine()
    {
        while (_isHolding) {
            _pressTime += Time.deltaTime;

            if (_pressTime >= KickHoldThreshold) {
                TryKickPlayer();
                _isHolding = false;
                yield break;
            }

            yield return null;
        }
    }

    private async void OnClick()
    {
        if (_player.CustomProperties.TryGetValue("UID", out object uidObj) && uidObj is string firebaseUid) {
            await PopupManager.Instance.ShowPlayerInfo(firebaseUid);
        }
        else {
            Debug.LogWarning("해당 플레이어의 UID가 등록되어 있지 않습니다.");
        }
    }

    private void TryKickPlayer()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (_player == PhotonNetwork.LocalPlayer)
            return;

        if (_player.CustomProperties.TryGetValue("UID", out object uidObj) && uidObj is string uid) {
            PopupManager.Instance.ShowOKCancelPopup("정말로 이 플레이어를 추방하시겠습니까?",
                "추방", () => {
                    PhotonManager.Instance.KickPlayer(_player, uid);
                    Debug.Log($"[Kick] UID {uid} 플레이어 추방 시도");
                },
                "취소", null);
        }
    }

    #endregion // private funcs
}
