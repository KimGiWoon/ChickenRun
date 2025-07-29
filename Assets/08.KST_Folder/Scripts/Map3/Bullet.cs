using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Bullet : MonoBehaviourPun
    {
        [SerializeField] private float _speed = 10f;
        private Vector2 _moveDir;

        //TODO <김승태> : Bullet끼리의 충돌 방지 조건 추가.
        public void Init(Vector2 dir)
        {
            _moveDir = dir.normalized;
        }

        void Update()
        {
            if (!photonView.IsMine) return;
            transform.Translate(_moveDir * _speed * Time.deltaTime);
        }

        //장애물과 충돌 시
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!photonView.IsMine)

                if (collision.CompareTag("Obstacle"))
                {
                    //TODO <김승태> : 추후 오브젝트 풀링 반납 구조로 변경 필요
                    //장애물 파괴 혹은 추가적인 장치 필요.
                    PhotonNetwork.Destroy(collision.gameObject);
                    //총알 파괴
                    PhotonNetwork.Destroy(gameObject);
                }
        }
    }

}