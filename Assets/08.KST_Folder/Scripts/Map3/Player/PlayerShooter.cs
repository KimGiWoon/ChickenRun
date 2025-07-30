using Photon.Pun;
using UnityEngine;

namespace Kst
{

    public class PlayerShooter : MonoBehaviourPun
    {
        [SerializeField] ArrowAim _arrow;
        [SerializeField] Transform _shootPos;
        [SerializeField] PooledObject _bulletPrefab;
        [SerializeField] private string _bulletPath = "Bullet";
        // private PhotonObjectPool _bulletPool;
        private ObjectPool _bulletPool;

        void Start()
        {
            _bulletPool = new(null, _bulletPrefab, 10);
            // if (PhotonNetwork.IsMasterClient)
            // _bulletPool = new PhotonObjectPool(null, _bulletPath, 10);
        }

        public void OnAttackBtn()
        {
            if (!photonView.IsMine) return;

            Vector2 shotDir = _arrow.GetDir();

            //마스터 클라이언트에게 poppool 호출 요청
            // PhotonPooledObject pooled = _bulletPool.PopPool();

            // //총알 위치설정
            // pooled.transform.SetPositionAndRotation(_shootPos.position, Quaternion.identity);

            // if (pooled.TryGetComponent(out PhotonView view))
            //     view.TransferOwnership(PhotonNetwork.LocalPlayer);

            // if (pooled.TryGetComponent(out Bullet bullet))
            //     bullet.Init(shotDir);
            photonView.RPC(nameof(RPC_ShootBullet), RpcTarget.AllViaServer, _shootPos.position, shotDir);


        }

        [PunRPC]
        void RPC_ShootBullet(Vector3 shootPos, Vector2 dir)
        {
            PooledObject bullet = _bulletPool.PopPool();

            bullet.transform.SetPositionAndRotation(shootPos, Quaternion.identity);
            if (bullet.TryGetComponent(out Bullet b))
                b.Init(dir);
        }
    }
}