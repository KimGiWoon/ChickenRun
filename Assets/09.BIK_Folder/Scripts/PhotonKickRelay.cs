using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonKickRelay : MonoBehaviourPun
{
    private System.Action _onKickedCallback;

    public void KickPlayer(Player targetPlayer, string uid)
    {
        photonView.RPC(nameof(RPC_ReceiveKickByUID), targetPlayer, uid);
    }

    public void SetOnKickedCallback(System.Action callback)
    {
        _onKickedCallback = callback;
    }

    [PunRPC]
    private void RPC_ReceiveKickByUID(string targetUid)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("UID", out object myUidObj)
            && myUidObj is string myUid
            && myUid == targetUid) {
            Debug.LogWarning("[Kick] 방장에 의해 추방됨");

            if (_onKickedCallback != null) {
                _onKickedCallback();
            }
            else {
                PhotonManager.Instance.OnKicked();
            }

            // relay가 파괴되기 전에 참조 끊기 (중요)
            Destroy(gameObject); // 또는 적절히 처리

            PhotonNetwork.LeaveRoom();

            // 팝업 호출 후 룸 퇴장
            PopupManager.Instance.ShowOKPopup("방장에 의해 추방되었습니다.", "확인");

        }
    }
}
