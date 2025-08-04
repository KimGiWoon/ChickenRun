using DanielLochner.Assets.SimpleScrollSnap;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectPanel : UIBase
{
    #region Serialized fields

    [SerializeField] private SimpleScrollSnap _scrollSnap;
    [SerializeField] private Button _okButton;
    [SerializeField] private GameObject _skinPrefab;

    #endregion // Serialized fields





    #region private fields

    private int _selectedSkinIndex = 0;

    #endregion // private fields





    #region mono funcs

    private void Start()
    {
        _okButton.onClick.AddListener(OnClickOK);
    }

    #endregion // mono funcs





    #region public funcs

    public override void SetShow()
    {
        base.SetShow();
        LoadSkinDataFromFirebase();
    }

    #endregion // public funcs





    #region private funcs

    private void LoadSkinDataFromFirebase()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid)) {
            Debug.LogError("[SkinSelect] Firebase UID가 없습니다.");
            return;
        }

        var skinRef = FirebaseDatabase.DefaultInstance.GetReference("UserData").Child(uid).Child("SkinData");

        skinRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || !task.Result.Exists) {
                Debug.LogWarning("[SkinSelect] 스킨 데이터 없음 또는 불러오기 실패");
                RefreshScrollSnap(new List<string>()); // Default만 보여주기
                return;
            }

            var snapshot = task.Result;
            List<string> ownedSkinNames = new();

            foreach (var skin in snapshot.Children) {
                string skinName = skin.Key;
                bool isPurchased = bool.TryParse(skin.Child("IsPurchased").Value?.ToString(), out var result) && result;

                if (isPurchased && skinName != "Default")
                    ownedSkinNames.Add(skinName);
            }

            RefreshScrollSnap(ownedSkinNames);
        });
    }

    private void RefreshScrollSnap(List<string> ownedSkins)
    {
        // 1. SimpleScrollSnap의 모든 기존 항목 제거
        while (_scrollSnap.NumberOfPanels > 0) {
            _scrollSnap.RemoveFromBack();
        }

        // 2. Default 스킨 추가
        AddSkinToScroll("Default");

        // 3. 나머지 스킨 추가
        foreach (var skinName in ownedSkins) {
            AddSkinToScroll(skinName);
        }

        // 4. 첫 번째로 이동 (SelectedPanel 초기화)
        _scrollSnap.GoToPanel(_selectedSkinIndex);
        //_selectedSkinIndex = 0;
    }

    private void AddSkinToScroll(string skinName)
    {
        // 1. AddToBack을 통해 프리팹 추가 (내부에서 Instantiate됨)
        _scrollSnap.AddToBack(_skinPrefab);

        // 2. 방금 추가된 패널 가져오기 (가장 마지막에 추가된 자식)
        int lastIndex = _scrollSnap.Content.childCount - 1;
        Transform newPanelTransform = _scrollSnap.Content.GetChild(lastIndex);
        //newPanelTransform.gameObject.SetActive(true); // <- 활성화
        newPanelTransform.name = skinName;

        // 3. Image 컴포넌트 가져오기
        Image imageInstance = newPanelTransform.GetComponentInChildren<Image>();
        if (imageInstance == null) {
            Debug.LogWarning($"[SkinSelect] Image component not found in prefab for skin: {skinName}");
            return;
        }

        // 4. Sprite 설정
        Sprite skinSprite = Resources.Load<Sprite>($"Sprites/Skins/{skinName}");
        if (skinSprite != null) {
            imageInstance.sprite = skinSprite;
        }
        else {
            Debug.LogWarning($"[SkinSelect] Sprite not found for skin: {skinName}");
        }
    }

    private void OnClickOK()
    {
        _selectedSkinIndex = _scrollSnap.SelectedPanel;

        string selectedSkinName = _scrollSnap.Content.GetChild(_selectedSkinIndex).name;
        Debug.Log($"[SkinSelect] 선택된 스킨: {selectedSkinName}");

        var props = new ExitGames.Client.Photon.Hashtable {
            { "Skin", selectedSkinName }
        };
        Photon.Pun.PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // TODO 백인권 : IF : 선택된 스킨이 Default가 아닐 때, Firebase에 저장하는 로직 추가 필요.

        SetHide();
    }

    #endregion // private funcs
}
