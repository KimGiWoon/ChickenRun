using UnityEngine;
using Firebase.Database;

namespace Kst
{
    public class EggController : MonoBehaviour
    {
        [SerializeField] private EggModel _eggModel;
        [SerializeField] private EggView _eggView;

        void OnEnable()
        {
            _eggModel.OnEggChanged += onEggChanged;
        }
        void OnDisable()
        {
            _eggModel.OnEggChanged -= onEggChanged;
        }
        void Start()
        {
            EggManager.Instance.Init(_eggModel);
        }

        void onEggChanged(int value)
        {
            _eggView.InitNormalEgg(value);
        }
        public void UseEgg(int amount)
        {
            _eggModel.DecreaseEgg(amount);
        }
        public void GainEgg(int amount)
        {
            _eggModel.IncreaseEgg(amount);
        }
        public int GetCurrentEgg()
        {
            return _eggModel.CurrentNormalEgg;
        }


    }
}