using UnityEngine;
using System.Collections.Generic;

namespace Kst
{
    public class ShopController : MonoBehaviour
    {
        #region 참조
        [SerializeField] private ShopView _shopView;
        [SerializeField] private ShopDataLoader _shopDataLoader;
        [SerializeField] private ShopDataBase _shopDatabase;
        // [SerializeField] private GoldManager _goldManager;
        [SerializeField] private EggController _goldManager;
        #endregion
        private List<SkinItemController> _itemControllers = new();

        private void Start()
        {
            _shopDataLoader.OnSkinDataLoaded += DataLoad;
            string uid = FirebaseManager.Auth.CurrentUser?.UserId;

            if (string.IsNullOrEmpty(uid))
                return;
                
            _shopDataLoader.LoadSkinsData(uid);
        }

        private void DataLoad(List<SkinData> list)
        {
            if (list == null) return;

            //초기화 진행
            _shopView.CleanUpItems();
            _itemControllers.Clear();

            foreach (SkinData data in list)
            {
                var view = _shopView.CreateItem();
                var controller = view.GetComponent<SkinItemController>();
                controller.Init(data, view, _goldManager, _shopDatabase);

                _shopView.AddItem(controller);
                _itemControllers.Add(controller);
            }
        }
    }
}