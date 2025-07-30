using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using Photon.Pun;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CYH_FirebaseManager : Singleton<CYH_FirebaseManager>
{
    private static FirebaseApp app;
    public static FirebaseApp App { get { return app; } }

    private static FirebaseAuth auth;
    public static FirebaseAuth Auth { get { return auth; } }

    private static FirebaseUser user;
    public static FirebaseUser User { get { return user; } }

    private static FirebaseDatabase database;
    public static FirebaseDatabase Database { get { return database; } }

    private static DatabaseReference dataReference;
    public static DatabaseReference DataReference { get { return dataReference; } }

    // 닉네임
    public static string CurrentUserNickname => Auth?.CurrentUser?.DisplayName ?? "게스트";

    // 구글
    [SerializeField] private string googleWebAPI = "1912177127-go83if3uk9pelsa2186ti52hu74qhv5g.apps.googleusercontent.com";

    private GoogleSignInConfiguration _configuration;
    public GoogleSignInConfiguration Configuration { get { return _configuration; } }

    // 테스트용 구글 로그인 버튼
    [SerializeField] private Button _GoogleButton;


    protected override void Awake()
    {
        _configuration = new GoogleSignInConfiguration {
            WebClientId = googleWebAPI,
            RequestIdToken = true,
            RequestEmail = true
        };

        GoogleSignIn.Configuration = _configuration;
    }

    private void Start()
    {
        // firebase 초기화
        StartCoroutine(InitFirebaseCoroutine());

        //로그인 초기화
        //if (auth.CurrentUser != null)
        //{
        //    auth.SignOut();
        //}

        _GoogleButton.onClick.AddListener(OnGoolgeSignInClicked);
    }

    private IEnumerator InitFirebaseCoroutine()
    {
        Task<Firebase.DependencyStatus> task = Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        Firebase.DependencyStatus dependencyStatus = task.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
            app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            database = FirebaseDatabase.DefaultInstance;
        }

        else {
            app = null;
            auth = null;
            database = null;
        }
    }

    public void OnGoolgeSignInClicked()
    {
        Debug.Log("구글 로그인 버튼 입력");

        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleAuthenticatedFinished);
    }

    public void OnFirebaseLoginSuccess()
    {
        user = auth.CurrentUser;

        if (!PhotonNetwork.IsConnected) {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[Photon] Firebase 로그인 이후 Photon 연결 시작");
        }
    }

    private void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsCanceled)
        {
            Debug.LogError("구글 인증 취소");
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"구글 인증 실패 : {task.Exception}");
        }

        else {
            GoogleLogin(task);
        }
    }

    private void GoogleLogin(Task<GoogleSignInUser> userTask)
    {
        Firebase.Auth.Credential credential =
        Firebase.Auth.GoogleAuthProvider.GetCredential(userTask.Result.IdToken, null);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("구글 로그인 취소");

                PopupManager.Instance.ShowOKPopup("구글 로그인 취소", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"구글 로그인 실패 : {task.Exception}");

                PopupManager.Instance.ShowOKPopup("구글 로그인 실패", "OK", () => PopupManager.Instance.HidePopup());
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            OnFirebaseLoginSuccess();

            PopupManager.Instance.ShowOKPopup("구글 로그인 성공", "OK", () => PopupManager.Instance.HidePopup());

            user = auth.CurrentUser;
        });
    }
}