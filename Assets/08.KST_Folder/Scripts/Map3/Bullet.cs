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
                    //TODO <김승태> : SFX 변경 필요
                    SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_DropWater); //총알과 플레이트 충돌 시 사운드
                }

                //TODO <김승태> : 해당 시점에 파괴 이펙트 생성 (반납은 알아서 되니깐 상관  x)

                //플레이트 반납 요청
                PlateMover mover = plate.GetComponentInChildren<PlateMover>();
                mover.ReturnPool();

                //총알 반납
                _pooledObj.ReturnPool();
            }
        }
    }
}