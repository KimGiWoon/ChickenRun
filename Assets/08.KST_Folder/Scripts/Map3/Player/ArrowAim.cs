using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class ArrowAim : MonoBehaviourPun
    {
        private float _angleRange = 45f; //회전 범위
        private float _rotateSpeed = 40f; //회전 속도

        [SerializeField] private Transform arrowStart;
        [SerializeField] private Transform arrowEnd;
        [SerializeField] private Transform _arrowPivot;

        private float _currentAngle = 0f; //현재 각도
        private bool _rotatingRight = true; // 우측 방향으로 회전 시

        void Update()
        {
            if (!photonView.IsMine) return;


            float _alpha = _rotateSpeed * Time.deltaTime;

            //우측 방향 회전
            if (_rotatingRight)
            {
                _currentAngle += _alpha;
                //현재 각도가 회전 범위 각 이상일 경우
                if (_currentAngle >= _angleRange)
                    _rotatingRight = false; //방향 변경
            }
            //좌측 방향 회전
            else
            {
                _currentAngle -= _alpha;
                if (_currentAngle <= -_angleRange)
                    _rotatingRight = true; //방향 변경
            }


            if (_arrowPivot != null)
                _arrowPivot.localRotation = Quaternion.Euler(0, 0, _currentAngle);
        }

        /// <summary>
        /// 방향을 벡터로 변환
        /// </summary>
        /// <returns></returns>
        public Vector2 GetDir()
        {
            Vector2 dir = (arrowEnd.position - arrowStart.position).normalized;
            return dir;
        }
    }
}