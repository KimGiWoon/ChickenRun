using Firebase.Auth;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartPanel : UIBase
{
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private Button _gameStartButton;

    public Action OnClickGameStart { get; set; }

    private void Start()
    {
        _gameStartButton.onClick.AddListener(() => OnClickGameStart?.Invoke());
        // 로비 씬으로 전환
        _gameStartButton.onClick.AddListener(() => SceneManager.LoadScene("MainScene"));

        Debug.Log("------유저 정보------");
        Debug.Log($"유저 닉네임 : {CYH_FirebaseManager.Auth.CurrentUser.DisplayName}");
        Debug.Log($"유저 ID : {CYH_FirebaseManager.Auth.CurrentUser.UserId}");
        Debug.Log($"이메일 : {CYH_FirebaseManager.Auth.CurrentUser.Email}");
    }

    private void OnEnable()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null && !string.IsNullOrEmpty(user.DisplayName))
        {
            _nicknameText.text = $"{user.DisplayName} 님";
        }
        else if (user.IsAnonymous)
        {
            //_nicknameText.text = $"{user.DisplayName} 님";
            _nicknameText.text = $"게스트 님";
        }
    }

    private void OnDisable()
    {
        _nicknameText.text = "";
    }
}
