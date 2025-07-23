using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

namespace Kst
{
    public class GoldManager : MonoBehaviour
    {
        private int _gold;
        private DatabaseReference _goldRef;
        [SerializeField] TMP_Text _goldtext;

        void OnEnable()
        {
            FirebaseManager.Instance.OnLogin += InitGold;
        }
        void OnDisable()
        {
            FirebaseManager.Instance.OnLogin -= InitGold;
            if (_goldRef != null) _goldRef.ValueChanged -= OnGoldValueChanged;
        }

        void InitGold()
        {
            string uid = FirebaseManager.User.UserId;
            _goldRef = FirebaseManager.DB.Child("UserData").Child(uid).Child("gold");

            _goldRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("골드 불러오기 실패");
                    return;
                }

                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists && int.TryParse(snapshot.Value.ToString(), out int result))
                {
                    _gold = result;
                    Debug.Log("골드 불러오기 성공");
                }
                else
                {
                    _gold = 0;
                    Debug.Log("골드 없음");
                    _goldRef.SetValueAsync(0);
                }
                _goldtext.text = $"{_gold}";
            });
            _goldRef.ValueChanged += OnGoldValueChanged;
        }


        void OnGoldValueChanged(object send, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Exists && int.TryParse(e.Snapshot.Value.ToString(), out int re))
            {
                _gold = re;
                _goldtext.text = $"{_gold}";
                Debug.Log("골드 갱신");
            }
        }

        //골드 증가 로직
        public void IncreaseGold()
        {
            if (!IsUserLogin()) return;
            if (!IsGoldRefInit()) return;

            _goldRef.RunTransaction(mutableData =>
            {
                int current = mutableData.Value == null ? 0 : int.Parse(mutableData.Value.ToString());
                current += 1;
                mutableData.Value = current;
                return TransactionResult.Success(mutableData);
            });
        }

        //골드 감소 로직
        public void DecreaseGold()
        {
            if (!IsUserLogin()) return;
            if (!IsGoldRefInit()) return;

            _goldRef.RunTransaction(mutableData =>
            {
                int current = mutableData.Value == null ? 0 : int.Parse(mutableData.Value.ToString());
                if (current <= 0)
                    return TransactionResult.Abort();

                current -= 1;
                mutableData.Value = current;
                return TransactionResult.Success(mutableData);
            });
        }

        //골드 데이터참조 초기화 확인
        private bool IsGoldRefInit()
        {
            if (_goldRef == null)
            {
                Debug.Log("골드 경로 참조 초기화 안됨");
                return false;
            }
            return true;
        }

        //로그인 여부 확인 로직
        private bool IsUserLogin()
        {
            if (FirebaseManager.User == null)
            {
                Debug.LogError("로그인 되어 있지 않음");
                return false;
            }
            return true;
        }
    }
}