using System.Collections.Generic;
using UnityEngine;

namespace Kst
{
    public class ShopController : MonoBehaviour
    {
        #region 참조
        [SerializeField] private ShopView _shopView;
        [SerializeField] private ShopDataLoader _shopDataLoader;
        [SerializeField] private ShopDataBase _shopDatabase;
        [SerializeField] private EggController _goldManager;
        #endregion
        private List<SkinItemController> _itemControllers = new();

        private void Start()
        {
            _shopDataLoader.OnSkinDataLoaded += DataLoad;
            string uid = CYH_FirebaseManager.Auth.CurrentUser?.UserId;

            if (string.IsNullOrEmpty(uid))
                return;

            _shopDataLoader.LoadSkinsData(uid);
        }

        /// <summary>
        /// 로딩된 데이터를 통해 상점 시스템 초기화 작업 진행
        /// </summary>
        /// <param name="list">Firebase에서 로드된 데이터 리스트</param>
        private void DataLoad(List<SkinData> list)
        {
            if (list == null)
                return;

            //초기화 진행
            _shopView.CleanUpItems();
            _itemControllers.Clear();

            int listCount = 0;

            foreach (SkinData data in list)
            {
                var view = _shopView.ActiveItem(listCount);
                var controller = view.GetComponent<SkinItemController>();
                controller.Init(data, view, _goldManager, _shopDatabase);

                _shopView.AddItem(controller);
                _itemControllers.Add(controller);
                ++listCount;
            }
        }
    }
}