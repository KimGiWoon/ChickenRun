using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Bullet : MonoBehaviourPun
    {
        [SerializeField] private float _speed = 10f;
        private Vector2 _moveDir;
        private PooledObject _pooledObj;

        void Awake()
        {
            _pooledObj = GetComponent<PooledObject>();
        }
        public void Init(Vector2 dir)
        {
            _moveDir = dir.normalized;
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
                if (plate.IsEggPlate())
                    EggManager.Instance.GainEgg(plate.GetEggAmount());
                else
                {
                    int score = plate.GetScore();
                    if (score > 0) ScoreManager.Instance.AddScroe(score);
                    else ScoreManager.Instance.MinusScore(-score);
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