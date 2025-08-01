using UnityEngine;

namespace Kst
{

    public class Plate : MonoBehaviour
    {
        [SerializeField] private PlateType _type;
        public PlateType GetPlateType() => _type;

        public int GetScore()
        {
            switch (_type)
            {
                case PlateType.NormalEgg:
                    return 0;
                case PlateType.Bomb:
                    return -2;
                case PlateType.Rock:
                    return 2;
                case PlateType.Coin:
                    return 10;
                default:
                    return 0;
            }
        }

        public bool IsEggPlate() => _type == PlateType.NormalEgg;
        public int GetEggAmount() => _type == PlateType.NormalEgg ? 1 : 0;

    }
}
