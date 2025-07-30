using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    #region Serialized fields

    [Header("UI")]
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private GameObject _readyOffIcon;
    [SerializeField] private GameObject _readyOnIcon;

    #endregion // Serialized fields





    #region private fields

    private Player _player;

    #endregion // private fields





    #region public funcs

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetPlayer(Player player)
    {
        _player = player;

        // 닉네임 설정 (CustomProperties에서 가져오고, 없으면 기본 Photon 닉네임 사용)
        if (player.CustomProperties.TryGetValue("Nickname", out object nicknameObj) && nicknameObj is string nickname) {
            _nicknameText.text = nickname;
        }
        else {
            _nicknameText.text = player.NickName; // 예외 처리용
        }

        // 닉네임 색깔 설정
        if (player.CustomProperties.TryGetValue("Color", out object colorObj) && colorObj is string colorStr) {
            if (System.Enum.TryParse(colorStr, out ColorType colorType)) {
                _nicknameText.color = Common.ConvertColorTypeToUnityColor(colorType);
            }
            else {
                _nicknameText.color = Color.black; // 기본 fallback
            }
        }
        else {
            _nicknameText.color = Color.black;
        }

        // 준비 상태 표시
        bool isReady = false;
        if (player.CustomProperties.TryGetValue("IsReady", out object readyValue) && readyValue is bool b) {
            isReady = b;
        }

        _readyOnIcon.SetActive(isReady);
        _readyOffIcon.SetActive(!isReady);
    }


    public async void OnClick()
    {
        if (_player.CustomProperties.TryGetValue("UID", out object uidObj) && uidObj is string firebaseUid) {
            await PopupManager.Instance.ShowPlayerInfo(firebaseUid);
        }
        else {
            Debug.LogWarning("해당 플레이어의 UID가 등록되어 있지 않습니다.");
        }
    }

    #endregion // public funcs
}
