// using UnityEngine;
// using Firebase;
// using Firebase.Auth;
// using Firebase.Database;
// using Firebase.Extensions;
// using System;
// using UnityEngine.SceneManagement;

// namespace Kst
// {
//     public class FirebaseManager : MonoBehaviour
//     {
//         public static FirebaseManager Instance { get; private set; }
//         public static FirebaseAuth Auth => Instance.auth;
//         public static FirebaseUser User => Instance.auth.CurrentUser;
//         public static DatabaseReference DB => Instance.database;

//         private FirebaseAuth auth;
//         private DatabaseReference database;

//         // public event Action OnLogin;
//         // [SerializeField] GameObject shopPanel,loginPanel;

//         private void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//                 DontDestroyOnLoad(gameObject);
//             }
//             else
//             {
//                 Destroy(gameObject);
//                 return;
//             }

//             FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
//             {
//                 if (task.Result == DependencyStatus.Available)
//                 {
//                     auth = FirebaseAuth.DefaultInstance;
//                     database = FirebaseDatabase.DefaultInstance.RootReference;
//                     Debug.Log("Firebase 초기화 완료");
//                 }
//                 else
//                 {
//                     Debug.LogError($"Firebase 초기화 실패: {task.Result}");
//                 }
//             });
//         }

//         public void LoginWithEmailAndPassword()
//         {
//             string email = "tiga1207@naver.com";
//             string password = "123456";

//             Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
//             {
//                 if (task.IsCanceled || task.IsFaulted)
//                 {
//                     Debug.LogError("로그인 실패: " + task.Exception);
//                 }
//                 else
//                 {
//                     Debug.Log("로그인 성공: " + Auth.CurrentUser.Email);
//                     // shopPanel.SetActive(true);
//                     // loginPanel.SetActive(false);
//                     // OnLogin.Invoke();
//                     SceneManager.LoadScene("Shop");
//                 }
//             });
//         }
//     }
// }