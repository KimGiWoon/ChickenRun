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

        public event Action<List<SkinData>> OnSkinDataLoaded; //TODO <김승태> private 선언 후 Init 하는 방식으로 추후 변경 필요.

        public void LoadSkinsData()
        {
            //Firebase 데이터베이스 값 검색 실시
            FirebaseDatabase.DefaultInstance.GetReference("ShopData/SkinData").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                //실패 혹은 존재하지 않을 시
                if (task.IsFaulted || !task.Result.Exists)
                {
                    OnSkinDataLoaded?.Invoke(null);
                    return;
                }

                DataSnapshot snapshot = task.Result;
                List<SkinData> loadDataList = new();

                //데이터 파싱 및 적용
                foreach (var skin in snapshot.Children)
                {
                    bool pasringImage = int.TryParse(skin.Child("ImageIndex").Value.ToString(), out int resultImageIndex);
                    bool parsingPrice = int.TryParse(skin.Child("Price").Value.ToString(), out int resultPrice);
                    string skinName = skin.Child("Name").Value?.ToString();

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

                    SkinData skinData = new()
                    {
                        ImageIndex = resultImageIndex,
                        Price = resultPrice,
                        SkinName = skinName,
                        IsPurchased = false //TODO <김승태> : 파이어베이스의 UserData에서 받아올 수 있도록 해야함.
                    };

                    //리스트에 데이터 추가.
                    loadDataList.Add(skinData);
                }
                OnSkinDataLoaded?.Invoke(loadDataList);
            });

        }


    }
}