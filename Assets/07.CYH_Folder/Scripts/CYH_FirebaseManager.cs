using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class CYH_FirebaseManager : MonoBehaviour
{
    // 앱 전체 설정과 상태 관리
    private static FirebaseApp app;
    public static FirebaseApp App { get { return app; } }

    // 인증 
    private static FirebaseAuth auth;
    public static FirebaseAuth Auth { get { return auth; } }

    // 데이터베이스
    private static FirebaseDatabase database;
    public static FirebaseDatabase Database { get { return database; } }


    private void Start()
    {
        //Firebase 의존성 상태를 검사하고 초기화
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            // 필요한 의존성 충족 검사
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Debug.Log("Firbase 사용 가능");
                // 앱을 사용할 수 있게 초기화
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                database = FirebaseDatabase.DefaultInstance;
            }
            else
            {
                Debug.LogError($"설정이 충족되지 않아 실패 / 원인: {dependencyStatus}");
                app = null;
                auth = null;
                database = null;
            }
        });
    }
}