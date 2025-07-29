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
    [SerializeField] private Image _skinPanel;

    #endregion // Serialized fields





    #region private fields

    private int _selectedSkinIndex = 0;

    #endregion // private fields





    #region mono funcs

    private void Start()
    {
        _okButton.onClick.AddListener(OnClickOK);
        _selectedSkinIndex = _scrollSnap.SelectedPanel;
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
        _scrollSnap.Remove(0); // 첫 항목 제거

        AddSkinToScroll("Default");

        foreach (var skinName in ownedSkins) {
            AddSkinToScroll(skinName);
        }

        _scrollSnap.GoToPanel(0);
    }

    private void AddSkinToScroll(string skinName)
    {
        Image imageInstance = Instantiate(_skinPanel, _scrollSnap.Content);
        imageInstance.name = skinName;

        Sprite skinSprite = Resources.Load<Sprite>($"Sprites/Skins/{skinName}");
        if (skinSprite != null) {
            imageInstance.sprite = skinSprite;
        }
        else {
            Debug.LogWarning($"[SkinSelect] Sprite not found for skin: {skinName}");
        }

        _scrollSnap.AddToBack(imageInstance.gameObject);
    }

    private void OnClickOK()
    {
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
