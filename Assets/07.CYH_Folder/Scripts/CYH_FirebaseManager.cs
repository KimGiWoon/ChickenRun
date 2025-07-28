using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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

    // 구글
    [SerializeField] private string googleWebAPI = "1912177127-go83if3uk9pelsa2186ti52hu74qhv5g.apps.googleusercontent.com";
    
    private GoogleSignInConfiguration _configuration;
    public GoogleSignInConfiguration Configuration { get { return _configuration; } } 

    
    [SerializeField] private TMP_Text _userNickname;
    [SerializeField] private TMP_Text _userEmail;
    [SerializeField] private TMP_Text _error;
    //[SerializeField] private Image userImage;

    private void Awake()
    {
        _configuration = new GoogleSignInConfiguration
        {
            WebClientId = googleWebAPI,
            RequestIdToken = true
        };
    }

    private void Start()
    {
        InitFirebase();
    }

    private void InitFirebase()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            Firebase.DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                database = FirebaseDatabase.DefaultInstance;
            }
            else
            {
                app = null;
                auth = null;
                database = null;
            }
        });
    }

    public void OnGoolgeSignInClicked()
    {
        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleAuthenticatedFinished);
    }

    private void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsCanceled)
        {
            Debug.Log("Login Cancel");
        }

        if (task.IsFaulted)
        {
            Debug.Log("Faulted");
        }

        else
        {
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
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                _error.text = "구글 로그인 취소";
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                _error.text = "구글 로그인 실패";
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            _error.text = "구글 로그인 성공";
            user = auth.CurrentUser;

            _userNickname.text = user.DisplayName;
            _userEmail.text = user.Email;

        });
    }
}