using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using Kst;
using UnityEngine;

public class SkinItemController : MonoBehaviour
{
    private SkinData _data;
    private SkinItemView _view;
    private EggController _goldManager;
    private ShopDataBase _database;

    /// <summary>
    /// SkinItemController의 초기화 메서드로,
    /// 데이터, UI 뷰, 골드매니저, 데이터베이스를 매개변수로 받아 UI 초기설정
    /// 구매 로직 및 버튼 이벤트 리스너 등록
    /// </summary>
    /// <param name="data">스킨 정보 데이터</param>
    /// <param name="view">해당 스킨에 대응하는 UI</param>
    /// <param name="goldManager">플레이어 골드 관리 매니저</param>
    /// <param name="shopDataBase">스킨 이미지 리소스 및 일부 정보를 갖고 있는 데이터베이스</param>
    // public void Init(SkinData data, SkinItemView view, GoldManager goldManager, ShopDataBase shopDataBase)
    public void Init(SkinData data, SkinItemView view, EggController goldManager, ShopDataBase shopDataBase)
    {
        _data = data;
        _view = view;
        _goldManager = goldManager;
        _database = shopDataBase;

        _view.SetData(
            _database.SkinInfoList[data.ImageIndex].SkinImage,
            data.SkinName,
            data.Price,
            data.IsPurchased
        );

        _view.RefreshUI();
        _view._purchaseBtn.onClick.AddListener(OnPurchaseClick);
    }
    /// <summary>
    /// 구매 버튼 클릭 시 팝업
    /// </summary>
    private void OnPurchaseClick()
    {
        Debug.Log("버튼 클릭");
        PopupManager.Instance.ShowOKCancelPopup(
            $"{_data.SkinName}을 {_data.Price}에 구매합니다.",
            "구매", TryBuy,
            "취소", null
        );
    }
    /// <summary>
    /// 구매 조건 검증 및 구매 진행
    /// </summary>
    private void TryBuy()
    {
        if (_goldManager.GetCurrentEgg() >= _data.Price)
        {
            string uid = CYH_FirebaseManager.Auth.CurrentUser?.UserId;
            if (!string.IsNullOrEmpty(uid))
            {
                _goldManager.UseEgg(_data.Price);
                _data.IsPurchased = true;

                var skinRef = FirebaseDatabase.DefaultInstance
                .GetReference("UserData").Child(uid).Child("SkinData").Child(_data.SkinName);

                Dictionary<string, object> skinData = new()
                {
                    {"IsPurchased", true},
                    {"PurchaseTime", DateTime.UtcNow.ToString("yy-MM-ddTHH:mm:ss")},
                    {"Price", _data.Price}
                };

                skinRef.UpdateChildrenAsync(skinData).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        _view.RefreshPurchase(true);
                        PopupManager.Instance.ShowOKPopup("구매완료");
                        Debug.Log("구매 성공");
                        _view.RefreshUI();
                    }
                });
            }
            else
            {
                Debug.LogError("로그인 유저 정보 없음");
            }
        }
        else
        {
            Debug.Log("잔액 부족");
            PopupManager.Instance.ShowOKPopup("골드 부족으로 인한 구매 실패");
        }
    }
}
