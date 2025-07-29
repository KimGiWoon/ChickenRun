using Photon.Pun;
using UnityEngine;

namespace Kst
{

    public class PlayerShooter : MonoBehaviourPun
    {
        [SerializeField] ArrowAim _arrow;
        [SerializeField] Transform _shootPos;
        [SerializeField] GameObject _bulletPrefab;

        public void OnAttackBtn()
        {
            if (!photonView.IsMine) return;

            Vector2 shotDir = _arrow.GetDir();

            //TODO <김승태> : 추후 오브젝트풀링 적용 해야 함.
            GameObject go = PhotonNetwork.Instantiate(_bulletPrefab.name, _shootPos.position, Quaternion.identity);
            go.GetComponent<Bullet>().Init(shotDir);
        }
    }
}