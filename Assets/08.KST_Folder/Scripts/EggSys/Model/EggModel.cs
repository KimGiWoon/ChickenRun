using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;

namespace Kst
{
    public class EggModel : MonoBehaviour
    {
        private int _normalEgg;
        public int CurrentNormalEgg { get { return _normalEgg; } set { _normalEgg = value; } }
        private DatabaseReference _normalEggRef;
        public DatabaseReference NormalEggRef { get { return _normalEggRef; } set { _normalEggRef = value; } }
        public event Action<int> OnEggChanged;

        void OnEnable() => StartCoroutine(DelaySubscribe());
        void OnDisable()
        {
            if (_normalEggRef != null)
                _normalEggRef.ValueChanged -= OnGoldValueChanged;
        }

        /// <summary>
        /// FirebaseManager의 유저가 없을 경우 대기 후 구독 처리
        /// </summary>
        private IEnumerator DelaySubscribe()
        {
            while (CYH_FirebaseManager.User == null)
                yield return null;

            InitEgg();
        }

        /// <summary>
        /// Model과 Firebase와 재화 연동
        /// </summary>
        void InitEgg()
        {
            string uid = CYH_FirebaseManager.User.UserId;
            _normalEggRef = CYH_FirebaseManager.DataReference.Child("UserData").Child(uid).Child("gold");

            //해당 유저의 재화 불러오기
            _normalEggRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("에그 불러오기 실패");
                    return;
                }

                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int result))
                {
                    _normalEgg = result;
                    Debug.Log("에그 불러오기 성공");
                }
                else
                {
                    _normalEgg = 0;
                    Debug.Log("에그 없음");
                    _normalEggRef.SetValueAsync(0);
                }

                //데이터베이스 연동 후 알림.
                OnEggChanged?.Invoke(_normalEgg);
            });

            //데이터베이스에서의 재화량 변경 시 이벤트 구독
            _normalEggRef.ValueChanged += OnGoldValueChanged;
        }

        /// <summary>
        /// 재화량 변경 시 재화량 갱신
        /// </summary>
        void OnGoldValueChanged(object send, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Exists && int.TryParse(e.Snapshot.Value.ToString(), out int re))
            {
                _normalEgg = re;
                OnEggChanged?.Invoke(_normalEgg);
                Debug.Log("에그 갱신");
            }
        }

        /// <summary>
        /// 골드 증가 로직
        /// </summary>
        /// <param name="amount"></param>
        public void IncreaseEgg(int amount)
        {
            if (!IsUserLogin() || !IsEggRefInit()) return;

            _normalEggRef.RunTransaction(mutableData =>
            {
                int current = mutableData.Value == null ? 0 : int.Parse(mutableData.Value.ToString());
                current += amount;
                mutableData.Value = current;
                return TransactionResult.Success(mutableData);
            });
        }

        /// <summary>
        /// 골드 감소 로직
        /// </summary>
        /// <param name="amount"></param>
        public void DecreaseEgg(int amount)
        {
            if (!IsUserLogin() || !IsEggRefInit()) return;

            _normalEggRef.RunTransaction(mutableData =>
            {
                int current = mutableData.Value == null ? 0 : int.Parse(mutableData.Value.ToString());
                if (current <= 0)
                    return TransactionResult.Abort();

                current -= amount;
                mutableData.Value = current;
                return TransactionResult.Success(mutableData);
            });
        }

        /// <summary>
        /// 골드 데이터참조 초기화 확인
        /// </summary>
        /// <returns></returns>
        private bool IsEggRefInit()
        {
            if (_normalEggRef == null)
            {
                Debug.Log("골드 경로 참조 초기화 안됨");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 로그인 여부 확인 로직
        /// </summary>
        /// <returns></returns>
        private bool IsUserLogin()
        {
            if (CYH_FirebaseManager.User == null)
            {
                Debug.LogError("로그인 되어 있지 않음");
                return false;
            }
            return true;
        }
    }
}