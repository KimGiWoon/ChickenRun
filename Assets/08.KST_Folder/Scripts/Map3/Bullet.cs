using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Bullet : MonoBehaviourPun
    {
        [SerializeField] private float _speed = 10f;
        private Vector2 _moveDir;
        // private PhotonPooledObject _pooledObj;
        private PooledObject _pooledObj;

        void Awake()
        {
            // _pooledObj = GetComponent<PhotonPooledObject>();
            _pooledObj = GetComponent<PooledObject>();
        }

        //TODO <김승태> : Bullet끼리의 충돌 방지 조건 추가.
        public void Init(Vector2 dir)
        {
            _moveDir = dir.normalized;
        }

        void Update()
        {
            // if (!photonView.IsMine) return;
            transform.Translate(_moveDir * _speed * Time.deltaTime);
        }

        //장애물과 충돌 시
        void OnTriggerEnter2D(Collider2D collision)
        {
            // if (!photonView.IsMine) return;

            if (collision.TryGetComponent(out Plate plate))
            {
                if (plate.IsEggPlate())
                    EggManager.Instance.GainEgg(plate.GetEggAmount());
                else
                {
                    int score = plate.GetScore();
                    if (score > 0) ScoreManager.Instance.AddScroe(score);
                    else ScoreManager.Instance.MinusScore(-score);
                }
                //플레이트 반납 요청

                // plate.GetComponent<PhotonView>().RPC(nameof(PlateMover.RPC_ReturnPlate), RpcTarget.MasterClient);

                //총알 반납
                // PhotonNetwork.Destroy(gameObject);
                _pooledObj.ReturnPool();
            }

        }
    }

}