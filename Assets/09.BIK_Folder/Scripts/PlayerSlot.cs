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

        _nicknameText.text = player.NickName;

        bool isReady = false;

        if (player.CustomProperties.TryGetValue("Ready", out object value)) {
            if (value is bool b)
                isReady = b;
        }

        _readyOnIcon.SetActive(isReady);
        _readyOffIcon.SetActive(!isReady);
    }

    public void OnClick()
    {
        if (_player.CustomProperties.TryGetValue("UID", out object uidObj) && uidObj is string firebaseUid) {
            PopupManager.Instance.ShowPlayerInfo(firebaseUid);
        }
        else {
            Debug.LogWarning("해당 플레이어의 UID가 등록되어 있지 않습니다.");
        }
    }

    #endregion // public funcs
}
