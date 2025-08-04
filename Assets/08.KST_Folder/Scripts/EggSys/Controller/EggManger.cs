using System;
using UnityEngine;

namespace Kst
{
    public class EggManager : Singleton<EggManager>
    {
        private EggModel _model;

        public void Init(EggModel model) => _model = model;

        public void GainEgg(int amount) => _model?.IncreaseEgg(amount);
        public void UseEgg(int amount) => _model?.DecreaseEgg(amount);
        

    }
}