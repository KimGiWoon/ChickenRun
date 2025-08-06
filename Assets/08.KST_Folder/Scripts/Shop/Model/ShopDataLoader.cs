using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System;

namespace Kst
{
    public class ShopDataLoader : MonoBehaviour
    {
        [SerializeField] private ShopDataBase _shopDataBase;

        public event Action<List<SkinData>> OnSkinDataLoaded;

        /// <summary>
        /// 상점 패널의 스킨 데이터 로딩 로직
        /// Firebase의 데이터베이스에서 상점 스킨 리스트와 유저 스킨 보유 리스트 불러오기
        /// ShopDataBase에서 이미지 리소스 연동
        /// </summary>
        /// <param name="uid">현재 로그인한 유저의 UID</param>
        public void LoadSkinsData(string uid)
        {
            var skinListRef = FirebaseDatabase.DefaultInstance.GetReference("ShopData/SkinData");
            var userskinRef = FirebaseDatabase.DefaultInstance.GetReference("UserData").Child(uid).Child("SkinData");

            //상점 스킨 리스트 불러오기
            skinListRef.GetValueAsync().ContinueWithOnMainThread(shopTask =>
            {
                //실패 혹은 존재하지 않을 시
                if (shopTask.IsFaulted || !shopTask.Result.Exists)
                {
                    OnSkinDataLoaded?.Invoke(null);
                    return;
                }
                DataSnapshot snapshot = shopTask.Result;

                //유저 스킨 정보 불러오기
                userskinRef.GetValueAsync().ContinueWithOnMainThread(userTask =>
                {
                    var userSkinDict = new Dictionary<string, bool>();

                    if (userTask.IsCompletedSuccessfully && userTask.Result.Exists)
                    {
                        foreach (var skin in userTask.Result.Children)
                        {
                            string skinName = skin.Key;
                            bool isPurchased = bool.TryParse(skin.Child("IsPurchased").Value?.ToString(), out var result) && result;
                            string purchaseTime = skin.Child("PurchaseTime").Value?.ToString();
                            userSkinDict[skinName] = isPurchased;
                        }
                    }

                    List<SkinData> loadDataList = new();

                    //데이터 파싱 및 적용
                    foreach (var skin in snapshot.Children)
                    {
                        string skinName = skin.Child("Name").Value?.ToString();
                        bool pasringImage = int.TryParse(skin.Child("ImageIndex").Value.ToString(), out int resultImageIndex);
                        bool parsingPrice = int.TryParse(skin.Child("Price").Value.ToString(), out int resultPrice);

                        //파싱 실패시 건너뛰기
                        if (!pasringImage || !parsingPrice || string.IsNullOrEmpty(skinName))
                        {
                            Debug.Log("파싱 실패");
                            continue;
                        }

                        //데이터가 유효하지 않을 경우 해당 snapshot 건너뛰기
                        if (resultImageIndex < 0 || resultImageIndex >= _shopDataBase.SkinInfoList.Count)
                        {
                            Debug.Log("데이터 유효성 문제 발생");
                            continue;
                        }

                        bool isPurchased = userSkinDict.TryGetValue(skinName, out var purchased) && purchased;

                        SkinData skinData = new()
                        {
                            ImageIndex = resultImageIndex,
                            Price = resultPrice,
                            SkinName = skinName,
                            IsPurchased = isPurchased
                        };

                        loadDataList.Add(skinData);
                    }
                    OnSkinDataLoaded?.Invoke(loadDataList);
                });
            });
        }
    }
}