using UnityEngine;
namespace Kst
{
    public class EggController : MonoBehaviour
    {
        [SerializeField] private EggModel _eggModel;
        [SerializeField] private EggView _eggView;

        void OnEnable() => _eggModel.OnEggChanged += OnEggChanged;
        void OnDisable() => _eggModel.OnEggChanged -= OnEggChanged;

        //EggManager와 Model 연결작업 실행
        void Start() => EggManager.Instance.Init(_eggModel);

        /// <summary>
        /// 변경된 값을 View에 전달하여 UI 변경
        /// </summary>
        /// <param name="value">재화 값</param>
        void OnEggChanged(int value) => _eggView.InitNormalEgg(value);

        /// <summary>
        /// 재화 사용 시 Model에서 재화 차감 로직 실행
        /// </summary>
        /// <param name="amount">차감 하고자 하는 재화량</param>
        public void UseEgg(int amount) => _eggModel.DecreaseEgg(amount);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>

        /// <summary>
        /// Model에서 데이터 값을 받아와 현재 모델에서 보유한 재화 수량 반환
        /// </summary>
        /// <returns></returns>
        public int GetCurrentEgg() => _eggModel.CurrentNormalEgg;
    }
}