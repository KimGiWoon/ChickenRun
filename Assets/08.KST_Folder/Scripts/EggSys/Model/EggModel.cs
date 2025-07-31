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

        void OnEnable()
        {
            StartCoroutine(DelaySubscribe());
        }
        void OnDisable()
        {
            if (_normalEggRef != null) _normalEggRef.ValueChanged -= OnGoldValueChanged;
        }

        private IEnumerator DelaySubscribe()
        {
            while (CYH_FirebaseManager.User == null)
                yield return null;


            InitEgg();
        }
        void InitEgg()
        {
            string uid = CYH_FirebaseManager.User.UserId;
            _normalEggRef = CYH_FirebaseManager.DataReference.Child("UserData").Child(uid).Child("gold");

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
                OnEggChanged?.Invoke(_normalEgg);
            });
            _normalEggRef.ValueChanged += OnGoldValueChanged;
        }
        void OnGoldValueChanged(object send, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Exists && int.TryParse(e.Snapshot.Value.ToString(), out int re))
            {
                _normalEgg = re;
                OnEggChanged?.Invoke(_normalEgg);
                Debug.Log("에그 갱신");
            }
        }
        //골드 증가 로직
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
        //골드 감소 로직
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

        //골드 데이터참조 초기화 확인
        private bool IsEggRefInit()
        {
            if (_normalEggRef == null)
            {
                Debug.Log("골드 경로 참조 초기화 안됨");
                return false;
            }
            return true;
        }

        //로그인 여부 확인 로직
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