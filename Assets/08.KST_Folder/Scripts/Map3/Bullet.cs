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

        //벽과 충돌 시
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
                _pooledObj.ReturnPool();
        }

        //장애물과 충돌 시
        void OnTriggerEnter2D(Collider2D collision)
        {
            //충돌 위치
            Vector3 hitTransform = transform.position;

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

                    //TODO <김승태> : 플레이트 별 sfx 다르게 출력하도록 soundmanager에서 사운드 참조걸기.
                    // 총알과 플레이트 충돌 시 플레이트 별 사운드 출력
                    switch (plate._type)
                    {
                        case PlateType.NormalEgg:
                            SoundManager.Instance.PlaySFX(SoundManager.Sfx_Plate_Sound.SFX_Egg);
                            break;
                        case PlateType.Rock:
                            SoundManager.Instance.PlaySFX(SoundManager.Sfx_Plate_Sound.SFX_Rock);
                            break;
                        case PlateType.Coin:
                            SoundManager.Instance.PlaySFX(SoundManager.Sfx_Plate_Sound.SFX_Coin);
                            break;
                        case PlateType.Bomb:
                            SoundManager.Instance.PlaySFX(SoundManager.Sfx_Plate_Sound.SFX_Bomb);
                            break;
                        default:
                            SoundManager.Instance.PlaySFX(SoundManager.Sfx_Plate_Sound.SFX_Egg);
                            break;
                    }
                }

                // 해당 시점에 파괴 이펙트 생성 (반납은 해당 애니메이션 끝 단 이벤트에서 실행)
                PooledObject effect = GameManager_Map3.Instance._effectPoolManager.GetPool();
                effect.transform.SetPositionAndRotation(hitTransform, Quaternion.identity);

                //플레이트 반납 요청
                PlateMover mover = plate.GetComponentInChildren<PlateMover>();
                mover.ReturnPool();

                //총알 반납
                _pooledObj.ReturnPool();
            }
        }
    }
}