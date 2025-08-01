using Firebase.Auth;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;


static partial class Utility
{
    /// <summary>
    ///  RankData/UserData 경로에 CurrentUse.DisplayName을 저장하는 메서드
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    public static void SaveNickname(FirebaseUser currentUser)
    {
        string uid = CYH_FirebaseManager.User.UserId;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary[$"UserData/{uid}/NickName"] = currentUser.DisplayName;
        dictionary[$"RankData/{uid}/NickName"] = currentUser.DisplayName;

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
}
