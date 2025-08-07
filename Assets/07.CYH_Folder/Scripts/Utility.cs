using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


static partial class Utility
{

    #region Save/Delete Nickname

    /// <summary>
    ///  RankData/UserData 경로에 CurrentUser.DisplayName을 저장하는 메서드
    /// </summary>
    public static void SaveNickname()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;
        string uid = currentUser.UserId;
        string userNickname = CYH_FirebaseManager.Auth.CurrentUser.DisplayName;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        // 익명계정 RankData 저장 x
        if (currentUser.IsAnonymous)
        {
            //dictionary[$"RankData/{uid}"] = new Dictionary<string, object>();
            dictionary[$"UserData/{uid}/Nickname"] = userNickname;
        }

        else
        {
            dictionary[$"UserData/{uid}/Nickname"] = userNickname;
            dictionary[$"RankData/{uid}/Nickname"] = userNickname;
        }

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

    public static async Task<bool> SaveNicknameAsync()
    {
        FirebaseUser currentUser = CYH_FirebaseManager.Auth.CurrentUser;
        string uid = currentUser.UserId;
        string userNickname = CYH_FirebaseManager.Auth.CurrentUser.DisplayName;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        // 익명계정 RankData 저장 x
        if (currentUser.IsAnonymous)
        {
            //dictionary[$"RankData/{uid}"] = new Dictionary<string, object>();
            dictionary[$"UserData/{uid}/Nickname"] = userNickname;
        }
        else
        {
            dictionary[$"UserData/{uid}/Nickname"] = userNickname;
            dictionary[$"RankData/{uid}/Nickname"] = userNickname;
        }
       
        var task = CYH_FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task; 

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("UserData / RankData 에 닉네임 저장 성공");
            return true;
        }
        else
        {
            Debug.LogError("닉네임 저장 실패");
            return false;
        }
    }

    /// <summary>
    /// RankData/UserData 경로의 현재 유저 Uid를 삭제하는 메서드
    /// </summary>
    public static void DeleteUserUID()
    {
        //string uid = CYH_FirebaseManager.User.UserId;
        string uid = CYH_FirebaseManager.Auth.CurrentUser.UserId;

        Debug.Log($" (DB Delete) 현재 로그인된 유저 uid : {uid}");
        Dictionary<string, object> dictionary = new Dictionary<string, object>
        {
            [$"UserData/{uid}"] = null,
            [$"RankData/{uid}"] = null
        };

        CYH_FirebaseManager.DataReference.UpdateChildrenAsync(dictionary).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("UserData / RankData 삭제 성공");
            }
            else
            {
                Debug.LogError($"UserData / RankData 삭제 실패: {task.Exception}");
            }
        });
    }

    /// <summary>
    /// Firebase DB UserData에서 현재 유저 닉네임을 불러오는 메서드
    /// 반환값: DB에 저장된 유저 Nickname
    /// </summary>
    public static async Task<string> LoadNickname()
    {
        string uid = CYH_FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference nicknameRef = CYH_FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Nickname");

        DataSnapshot snapshot = await nicknameRef.GetValueAsync();
        string nickname = snapshot.Value.ToString();
        Debug.Log($"LoadNickname 닉네임 : {nickname}");

        if (snapshot.Exists)
        {
            //string nickname = snapshot.Value.ToString();
            Debug.Log($"닉네임 로드 성공 : {nickname}");
            return nickname;
        }
        else
        {
            Debug.LogWarning("닉네임 데이터 없음");
            return string.Empty;
        }
    }

    #endregion

    /// <summary>
    /// 유저 온라인 체크 및 상태 저장
    /// IsOnline = true
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> IsOnline()
    {
        string uid = CYH_FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef = CYH_FirebaseManager.DataReference.Child("UserData").Child(uid).Child("IsOnline");

        Debug.Log("IsOnline() 호출 완료");

        // 현재 접속 상태 확인
        DataSnapshot snapshot = await userRef.GetValueAsync();
        bool isOnline = snapshot.Exists && snapshot.Value.ToString() == "true";
        if (isOnline)
        {
            Debug.LogError("IsOnline: 이미 로그인된 계정");
            return false;
        }

        // 로그인 -> IsOnline = true로 설정
        // disconnect -> 자동 false
        await userRef.SetValueAsync(true);
        Debug.Log($"로그인 / 유저 UID : {uid} IsOnline: {userRef}");
        
        await userRef.OnDisconnect().SetValue(false);

        return true;
    }

    /// <summary>
    /// 유저 로그아웃 시 호출
    /// IsOnline = false
    /// </summary>
    /// <returns></returns>
    public static async Task IsSetOffline()
    {
        string uid = CYH_FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef = CYH_FirebaseManager.DataReference.Child("UserData").Child(uid).Child("IsOnline");

        await userRef.SetValueAsync(false);
        Debug.Log($"로그아웃  / 유저 UID : {uid} IsOnline: {userRef}");
    }


    #region SetNickname

    /// <summary>
    /// 익명계정의 DisplayName을 "게스트 + 랜덤숫자"로 변경하는 메서드 
    /// 연결: GuestLogin
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    public static void SetNickname(FirebaseUser currentUser)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = currentUser.DisplayName;

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

                // Firebase DB에 닉네임 저장
                SaveNickname();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }
    

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

                // Firebase DB에 닉네임 저장
                SaveNickname();

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

                // Firebase DB에 닉네임 저장
                SaveNickname();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }

    /// <summary>
    /// 익명계정의 DisplayName을 "게스트 + 랜덤숫자"로 변경하는 메서드 
    /// 연결: GuestLogin
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    public static async Task SetGuestNickname(FirebaseUser currentUser)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = $"게스트{Random.Range(1000, 10000)}";

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(async task =>
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

                // Firebase DB에 닉네임 저장
                await SaveNicknameAsync();

                await currentUser.ReloadAsync();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }

    /// <summary>
    ///  익명계정에서 구글계정으로 전환된 유저의 DisplayName을 구글계정 DisplayName으로 변경하는 메서드 
    /// 연결: LinkPanel
    /// </summary>
    /// <param name="currentUser">닉네임을 변경할 유저</param>
    /// <param name="googleDisplayName">새로 설정할 닉네임(구글 계정 닉네임)</param>
    public static async Task SetGoogleNickname(FirebaseUser currentUser ,string googleDisplayName)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = googleDisplayName;
        Debug.Log($"SetGoogleNickname : googleDisplayName = {googleDisplayName}" );

        currentUser.UpdateUserProfileAsync(profile)
            .ContinueWithOnMainThread(async task =>
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
                //currentUser.ReloadAsync();

                // Firebase DB에 닉네임 저장
                await SaveNicknameAsync();
                await currentUser.ReloadAsync();

                Debug.Log("닉네임 설정 성공");
                Debug.Log($"변경된 유저 닉네임 : {currentUser.DisplayName}");
            });
    }

    #endregion
}
