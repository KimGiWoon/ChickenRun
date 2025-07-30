using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Map3_PlayerController : MonoBehaviourPun
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private PhotonView _pv;
        [SerializeField] private float _movespeed = 5f;
        private float _moveDir = 0f;
        void Start()
        {
            if (!photonView.IsMine) return;

            Camera.main.GetComponent<CameraController>().SetTarget(transform);
        }

        void FixedUpdate()
        {
            if (!_pv.IsMine) return;

            _rb.velocity = new Vector2(_moveDir * _movespeed, _rb.velocity.y);
        }
        public void SetDir(int dir)
        {
            if (!_pv.IsMine) return;
            _moveDir = dir;
        }
    }
}