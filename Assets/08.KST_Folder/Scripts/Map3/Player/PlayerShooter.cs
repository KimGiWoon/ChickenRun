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
        private ObjectPool _bulletPool;

        void Start()
        {
            _bulletPool = new(null, _bulletPrefab, 10);

        }

        public void OnAttackBtn()
        {
            if (!photonView.IsMine) return;

            Vector2 shotDir = _arrow.GetDir();
            int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            photonView.RPC(nameof(RPC_ShootBullet), RpcTarget.AllViaServer, _shootPos.position, shotDir, actorNum);

        }

        [PunRPC]
        void RPC_ShootBullet(Vector3 shootPos, Vector2 dir, int actorNum)
        {
            PooledObject bullet = _bulletPool.PopPool();

            bullet.transform.SetPositionAndRotation(shootPos, Quaternion.identity);
            if (bullet.TryGetComponent(out Bullet b))
                b.Init(dir,actorNum);
        }
    }
}