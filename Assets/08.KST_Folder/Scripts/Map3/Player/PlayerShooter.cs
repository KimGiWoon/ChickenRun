using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Kst
{

    public class PlayerShooter : MonoBehaviourPun
    {
        [SerializeField] ArrowAim _arrow;
        [SerializeField] Transform _shootPos;
        [SerializeField] PooledObject _bulletPrefab;
        private ObjectPool _bulletPool;
        private bool _canAttack = false;
        private Image _cooldownImg;

        void Start() => _bulletPool = new(null, _bulletPrefab, 10);

        public void OnAttackBtn()
        {
            if (!photonView.IsMine) return;
            if (!_canAttack) return;

            Vector2 shotDir = _arrow.GetDir();
            int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            photonView.RPC(nameof(RPC_ShootBullet), RpcTarget.AllViaServer, _shootPos.position, shotDir, actorNum);
            
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Shot); //총알 발사 시 SFX 실행
            StartCoroutine(IE_Cooldown());
        }

        [PunRPC]
        void RPC_ShootBullet(Vector3 shootPos, Vector2 dir, int actorNum)
        {
            PooledObject bullet = _bulletPool.PopPool();

            bullet.transform.SetPositionAndRotation(shootPos, Quaternion.identity);
            if (bullet.TryGetComponent(out Bullet b))
                b.Init(dir, actorNum);
        }

        IEnumerator IE_Cooldown()
        {
            _canAttack = false;

            float timer = 1f;
            float duration = 0f;
            _cooldownImg.fillAmount = 1f;

            while (duration < timer)
            {
                duration += Time.deltaTime;
                _cooldownImg.fillAmount = 1f - (duration / timer);
                yield return null;
            }
            // yield return new WaitForSeconds(1f);
            SetCanAttack();
        }

        public void SetCanAttack()
        {
            _canAttack = true;
            _cooldownImg.fillAmount = 0f;
        }
        public void SetImg(Image img) => _cooldownImg = img;
    }
}