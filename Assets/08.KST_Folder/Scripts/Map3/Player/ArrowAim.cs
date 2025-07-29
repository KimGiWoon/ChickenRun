using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class ArrowAim : MonoBehaviourPun
    {
        [SerializeField] private float _angleRange = 45f;
        [SerializeField] private float _rotateSpeed = 40f;
        [SerializeField] private Transform arrowStart;
        [SerializeField] private Transform arrowEnd;
        [SerializeField] private Transform _arrowPivot;

        private float _currentAngle = 0f;
        private bool _rotatingRight = true;// 우측 방향으로 회전 시

        void Update()
        {
            if (!photonView.IsMine) return;
            float _alpha = _rotateSpeed * Time.deltaTime;
            if (_rotatingRight)
            {
                _currentAngle += _alpha;
                if (_currentAngle >= _angleRange)
                    _rotatingRight = false;
            }
            else
            {
                _currentAngle -= _alpha;
                if (_currentAngle <= -_angleRange)
                    _rotatingRight = true;
            }
            if (_arrowPivot != null)
                _arrowPivot.localRotation = Quaternion.Euler(0, 0, _currentAngle);
        }

        //방향을 벡터로 변환
        public Vector2 GetDir()
        {
            Vector2 dir = (arrowEnd.position - arrowStart.position).normalized;
            return dir;
        }
    }
}