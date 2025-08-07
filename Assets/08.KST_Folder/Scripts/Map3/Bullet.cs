using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        private Vector2 _moveDir;
        private PooledObject _pooledObj;
        private int _actorNum;
        [SerializeField] SpriteRenderer _bulletSr;

        void Awake() => _pooledObj = GetComponent<PooledObject>();

        void Update() => transform.Translate(_moveDir * _speed * Time.deltaTime);

        /// <summary>
        /// 총알 방향 및 발사자 초기화 로직
        /// </summary>
        /// <param name="dir">총알의 방향 벡터 </param>
        /// <param name="actorNum">발사자(포톤 네트워크 환경에서 발사한 자의 액터넘버)</param>
        public void Init(Vector2 dir, int actorNum)
        {
            _moveDir = dir.normalized;
            _actorNum = actorNum;

            if (_actorNum == PhotonNetwork.LocalPlayer.ActorNumber)
                SetAlpha(1f);
            else
                SetAlpha(0.5f);

            StartCoroutine(AutoReturn());
        }

        private void SetAlpha(float amount)
        {
            if (_bulletSr != null)
            {
                Color color = _bulletSr.color;
                color.a = amount;
                _bulletSr.color = color;
            }
        }

        /// <summary>
        /// 5초 후 총알 자동 반납
        /// </summary>
        /// <returns></returns>
        IEnumerator AutoReturn()
        {
            yield return new WaitForSeconds(5f);

            if (gameObject.activeSelf)
                _pooledObj.ReturnPool();
        }

        /// <summary>
        /// 벽과 충돌 시 총알 반납
        /// </summary>
        /// <param name="collision">Wall 태그 가진 오브젝트에 한하여 로직 실행</param>
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
                _pooledObj.ReturnPool();
        }

        /// <summary>
        /// Plate와 충돌 시 점수지급 및 반납 로직
        /// </summary>
        /// <param name="collision">trigger인 콜라이더 중 Plate 컴포넌트를 가진 게임 오브젝트</param>
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out Plate plate))
            {
                //충돌 위치
                Vector3 hitTransform = transform.position;

                //마스터 클라이언트에서 점수 및 에그(재화) 지급
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

                    // 총알과 플레이트 충돌 시 플레이트 타입 별 사운드 출력
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