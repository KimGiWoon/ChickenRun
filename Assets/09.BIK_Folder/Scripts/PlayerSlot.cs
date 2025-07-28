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

    // 나중에 클릭 시 정보창 표시할 경우 사용
    public void OnClick()
    {
        // TODO백인권: PlayerInfoPopup.Instance.Show(_player);
    }

    #endregion // public funcs
}
