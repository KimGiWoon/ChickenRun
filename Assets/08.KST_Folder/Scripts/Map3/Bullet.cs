using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Bullet : MonoBehaviourPun
    {
        [SerializeField] private float _speed = 10f;
        private Vector2 _moveDir;
        private PooledObject _pooledObj;
        private int _actorNum;

        void Awake()
        {
            _pooledObj = GetComponent<PooledObject>();
        }
        public void Init(Vector2 dir, int actorNum)
        {
            _moveDir = dir.normalized;
            _actorNum = actorNum;
        }

        void Update()
        {
            transform.Translate(_moveDir * _speed * Time.deltaTime);
        }

        //장애물과 충돌 시
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Plate plate))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (plate.IsEggPlate())
                        ScoreManager.Instance.photonView.RPC(nameof(ScoreManager.GiveEgg), RpcTarget.All, _actorNum, plate.GetEggAmount());
                    else
                    {
                        int score = plate.GetScore();
                        if (score > 0)
                            ScoreManager.Instance.photonView.RPC(nameof(ScoreManager.AddScore), RpcTarget.All, _actorNum, plate.GetScore());
                        else
                            ScoreManager.Instance.photonView.RPC(nameof(ScoreManager.MinusScore), RpcTarget.All, _actorNum, plate.GetScore());
                    }
                }

                //플레이트 반납 요청
                PlateMover mover = plate.GetComponentInChildren<PlateMover>();
                mover.ReturnPool();

                //총알 반납
                _pooledObj.ReturnPool();
            }
        }
    }
}