using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Google;
using Photon.Pun;
using System.Collections;
using System.Threading.Tasks;
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

    private static DatabaseReference dataReference;
    public static DatabaseReference DataReference { get { return dataReference; } }

    // firebase 초기화 완료 여부 체크 플래그
    private bool _isFirebaseReady = false;   

    public bool IsFirebaseReady           
    {
        get { return _isFirebaseReady; }
        private set { _isFirebaseReady = value; }
    }

    // 닉네임
    public static string CurrentUserNickname => Auth?.CurrentUser?.DisplayName ?? "게스트";

    // 구글
    [SerializeField] private string googleWebAPI = "1912177127-go83if3uk9pelsa2186ti52hu74qhv5g.apps.googleusercontent.com";

    private GoogleSignInConfiguration _configuration;
    public GoogleSignInConfiguration Configuration { get { return _configuration; } }


    protected override void Awake()
    {
        // GoogleSignIn에 사용할 인증 설정 초기화
        _configuration = new GoogleSignInConfiguration
        {
            WebClientId = googleWebAPI,
            RequestIdToken = true,
            RequestEmail = true
        };

        // 초기화한 설정을 GoogleSignIn.Configuration에 적용
        GoogleSignIn.Configuration = _configuration;
    }

    private void Start()
    {
        // firebase 초기화
        StartCoroutine(InitFirebaseCoroutine());
    }

    /// <summary>
    /// Firebase 의존성 체크 후 각 인스턴스를 초기화하는 코루틴
    /// </summary>
    private IEnumerator InitFirebaseCoroutine()
    {
        Task<Firebase.DependencyStatus> task = Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        Firebase.DependencyStatus dependencyStatus = task.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            database = FirebaseDatabase.DefaultInstance;
            dataReference = FirebaseDatabase.DefaultInstance.RootReference;

            // 게임 시작 시 자동 로그아웃
            //if (auth != null && auth.CurrentUser != null)
            //{
            //    auth.SignOut();
            //    Debug.Log("auth.CurrentUser != null : 로그아웃");
            //}
        }

        else
        {
            app = null;
            auth = null;
            database = null;
            dataReference = null;
        }

        IsFirebaseReady = true;
        Debug.Log($"IsFirebaseReady : {IsFirebaseReady}");
    }

    public void OnFirebaseLoginSuccess()
    {
        user = auth.CurrentUser;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[Photon] Firebase 로그인 이후 Photon 연결 시작");
        }
    }

    /// <summary>
    /// 게임 실행 시 CurrentUser(로그인 유저) 여부를 체크하는 메서드
    /// true: 로그인된 유저가 있음 (자동 로그인 상태) -> GameStartPanel
    /// false: 로그인된 유저가 없음 (로그인 필요) -> LoginPanel
    /// </summary>
    /// <returns></returns>
    public bool IsLoggedIn()
    {
        Debug.Log($"IsLoggedIn 실행");
        return (auth != null && auth.CurrentUser != null) ? true : false;
    }
}