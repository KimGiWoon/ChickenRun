using System.Collections.Generic;
using UnityEngine;

namespace Kst
{
    public class ShopView : UIBase
    {
        #region UI참조
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private List<SkinItemView> _skinItemPrefab;
        #endregion

        private List<SkinItemView> _skinViews = new();

        public void CleanUpItems()
        {
            //스크롤뷰 컨텐츠 하위 오브젝트 초기화
            foreach (Transform child in _contentTransform)
                //Destroy(child.gameObject);

                { }
            _skinViews.Clear();
        }

        public SkinItemView ActiveItem(int index)
        {
            if (index < 0 || index >= _skinItemPrefab.Count) {
                return null;
            }

            SkinItemView item = _skinItemPrefab[index];
            item.gameObject.SetActive(true);

            _skinViews.Add(item);

            return item;

            //GameObject go = Instantiate(_skinItemPrefab, _contentTransform);
            //var view = go.GetComponent<SkinItemView>();
            //_skinViews.Add(view);
            //return view;
        }

        public void AddItem(SkinItemController controller)
        {
            controller.transform.SetParent(_contentTransform, false);
        }

    }
}