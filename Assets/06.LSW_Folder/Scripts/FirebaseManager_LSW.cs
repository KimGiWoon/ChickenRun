using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

// DB 접근을 위한 임시 FirebaseManager
public class FirebaseManager_LSW : Singleton<FirebaseManager_LSW>
{
    private static FirebaseApp _app;
    public static FirebaseApp App { get { return _app; } }

    private static FirebaseAuth _auth;
    public static FirebaseAuth Auth { get { return _auth; } }
    
    private static FirebaseDatabase _database;
    public static FirebaseDatabase Database { get { return _database; } }
    
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("파이어베이스 등록");
                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _database = FirebaseDatabase.DefaultInstance;
                
                _database.GoOnline();
            }
            else
            {
                _app = null;
                _auth = null;
                _database = null;
            }
        });
    }
}
