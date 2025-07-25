using TMPro;
using UnityEngine;

namespace Kst
{
    public class EggView : UIBase
    {
        [SerializeField] private TMP_Text _normalEggText;
        private int _currentNormalEgg;

        public void InitNormalEgg(int egg)
        {
            _currentNormalEgg = egg;
            RefreshUI();
        }
        public override void RefreshUI()
        {
            if (_normalEggText != null)
                _normalEggText.text = $"{_currentNormalEgg}";
        }
    }
}