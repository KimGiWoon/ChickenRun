using Firebase.Auth;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;


static partial class Utility
{

    #region SaveNickname

    /// <summary>
    ///  RankData/UserData 경로에 CurrentUse.DisplayName을 저장하는 메서드
    /// </summary>
    public static void SaveNickname()
    {
        string uid = CYH_FirebaseManager.User.UserId;
        string userNickname = CYH_FirebaseManager.Auth.CurrentUser.DisplayName;


        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary[$"UserData/{uid}/NickName"] = userNickname;
        dictionary[$"RankData/{uid}/NickName"] = userNickname;

        CYH_FirebaseManager.DataReference.UpdateChildrenAsync(dictionary).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("UserData / RankData 에 닉네임 저장 성공");
            }
            else
            {
                Debug.LogError("닉네임 저장 실패");
            }
        });
    }

    #endregion

    #region SetNickname

    /// <summary>
    /// 유저의 DisplayName을 입력받은 값으로 변경하는 메서드 
    /// 연결: AccountPanel - 닉네임 변경 기능
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    public static void SetNickname(string newNickname)
    {
        UserProfile profile = new UserProfile();
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;
        profile.DisplayName = newNickname;

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("닉네임 설정 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("닉네임 설정 실패");
                    return;
                }

                // 초기화
                currentUser.ReloadAsync();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }

    /// <summary>
    /// 익명계정의 DisplayName을 "게스트 + 랜덤숫자"로 변경하는 메서드 
    /// 연결: GuestLogin
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    public static void SetNickname(FirebaseUser currentUser)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = $"게스트{Random.Range(1000, 10000)}";

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("닉네임 설정 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("닉네임 설정 실패");
                    return;
                }

                // 초기화
                currentUser.ReloadAsync();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }

    /// <summary>
    /// 익명계정에서 구글계정으로 전환된 유저의 DisplayName을 구글계정 DisplayName으로 변경하는 메서드 
    /// 연결: LinkPanel
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    /// <param name="googleDisplayName">새로 설정할 닉네임(구글 계정 닉네임)</param>
    public static void SetNickname(FirebaseUser currentUser, string googleDisplayName)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = googleDisplayName;

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("닉네임 설정 취소");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("닉네임 설정 실패");
                    return;
                }
                
                // 초기화
                currentUser.ReloadAsync();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }

    #endregion
}
