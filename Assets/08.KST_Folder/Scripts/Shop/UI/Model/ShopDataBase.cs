using System.Collections.Generic;
using UnityEngine;

namespace Kst
{
    [CreateAssetMenu(fileName = "ShopDataBase", menuName = "Shop/Database")]
    public class ShopDataBase : ScriptableObject
    {
        //스킨 이미지 리소스 및 스킨 이름을 저장하는 리스트
        public List<SkinInfo> SkinInfoList;
    }
}