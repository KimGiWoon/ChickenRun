using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    #region Serialized fields

    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private TextMeshProUGUI _roomNameText;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    #endregion // Serialized fields





    #region Private fields

    private string _password = "";
    private string _roomName = "";

    #endregion // Private fields





    #region public funcs

    public void Initialize(RoomInfo info)
    {
        _roomName = info.Name;

        string displayRoomName = info.CustomProperties.ContainsKey("RoomName")
            ? (string)info.CustomProperties["RoomName"]
            : "(알 수 없음)";

        // 비밀번호 잠금 여부 확인
        bool isLocked = info.CustomProperties.ContainsKey("Password") &&
                        !string.IsNullOrEmpty((string)info.CustomProperties["Password"]);
        _password = isLocked ? (string)info.CustomProperties["Password"] : "";

        _roomNameText.text = displayRoomName;
        _playerCountText.text = $"[{info.PlayerCount}/{info.MaxPlayers}]";
        _lockIcon.SetActive(isLocked);
    }

    public void OnClick()
    {
        if (string.IsNullOrEmpty(_password)) { // 비밀번호가 없는 경우 바로 방에 입장
            PhotonManager.Instance.JoinRoom(_roomName);
        }
        else { // 비밀번호가 있는 경우 비밀번호 입력 팝업 표시 TODO : 비밀번호 팝업
            // 비밀번호 입력 팝업 요청
            //PopupManager.Instance.ShowPasswordPopup("비밀번호를 입력해주세요.", _password, () => {
            //    PhotonManager.Instance.JoinRoom(_roomName);
            //});
        }
    }

    #endregion // public funcs
}
