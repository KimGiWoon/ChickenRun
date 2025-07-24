using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun.Demo.Cockpit;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Database_RecordManager : Singleton<Database_RecordManager>
{
    [SerializeField] private Button _openBtn;
    [SerializeField] private Button _testBtn;
    [SerializeField] private Button _loadBtn;
    [SerializeField] private GameObject _rankPanel;
    
    private FirebaseAuth _auth;
    private DatabaseReference _reference;
    
    private RankData _rankData;

    // DTO (Data Transfer Object) 
    // 데이터만 필드로 가지고 있는 클래스
    [System.Serializable]
    public class RankData
    {
        public string Nickname;
        public long Map1Record;
        public long Map2Record;
        public long Map3Record;
        public long Score;
    }

    public class UserRankInfo
    {
        public int Rank;
        public string Nickname;
        public string RecordOrScore;
    }
    
    protected override void Awake()
    {
        base.Awake();
        _openBtn.onClick.AddListener(Open);
        _testBtn.onClick.AddListener(Test);
    }
    
    // 테스트용 자동 로그인 코드
    private void Start()
    {
        string email = "hagwhr2@gmail.com";
        string pass = "dltjrdnjs96~";
        
        _auth = FirebaseManager_LSW.Auth;
        if (_auth == null)
        {
            _auth = FirebaseAuth.DefaultInstance;
        }

        _auth.SignInWithEmailAndPasswordAsync(email, pass).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                _reference = FirebaseManager_LSW.Database.RootReference;
                Debug.Log("로그인 성공");
            }
            else
            {
                Debug.Log($"로그인 실패 : {task.Exception}");
            }
        });
    }

    // 기록을 어떤 방식으로 저장하는 것이 비용이 가장 적게 들어가는가?
    // MM:SS:SS 를 그대로 캐싱하는 것은 아무리 생각해도 비용이 높을 것 같음
    // 밀리초(ms)를 사용하면 차라리 ms를 정수값으로 저장하고
    // 이를 포맷을 바꿔서 UI에 노출시키는게 가장 낫지 않을까
    private string FormatData(int ms)
    {
        int minute = ms / 60000;
        int second = (ms % 60000) / 1000;
        int millisecond = ms % 1000 / 10;
        return $"{minute:D2}:{second:D2}:{millisecond:D2}";
    }
    
    public void LoadRecordRank(string record, GameObjectPool boardPool)
    {
        _reference.Child("RankData")
            .OrderByChild(record)
            .LimitToFirst(20)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted) return;
                DataSnapshot snapshots = task.Result;

                int rank = 1;
                foreach (var snapshot in snapshots.Children)
                {
                    RankData rankData = JsonUtility.FromJson<RankData>(snapshot.GetRawJsonValue());
                    int recordValue = 0;
                    switch (record)
                    {
                        case "Map1Record" :
                            recordValue = (int)rankData.Map1Record;
                            break;
                        case "Map2Record" :
                            recordValue = (int)rankData.Map2Record;
                            break;
                        case "Map3Record" :
                            recordValue = (int)rankData.Map3Record;
                            break;
                    }

                    GameObject board = boardPool.GetPool();
                    board.GetComponent<UserPersonalRecord>().SetRecordText(rank,rankData.Nickname,FormatData(recordValue));
                    rank++;
                }
            });
    }
    
    public void LoadScoreRank(GameObjectPool boardList)
    {
        FirebaseUser user = _auth.CurrentUser;
        _reference.Child("RankData")
            .OrderByChild("Score")
            .LimitToLast(20)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted) return;
                DataSnapshot snapshots = task.Result;

                int rank = 1;
                foreach (var snapshot in snapshots.Children.Reverse())
                {
                    RankData rankData = JsonUtility.FromJson<RankData>(snapshot.GetRawJsonValue());
                    GameObject board = boardList.GetPool();
                    board.GetComponent<UserPersonalRecord>().SetScoreText(rank,rankData.Nickname,rankData.Score);
                    rank++;
                }
            });
    }

    public async Task<UserRankInfo> LoadUserRank(string record)
    {
        RankData userRankData = await LoadRankData();
        UserRankInfo info = new UserRankInfo();
        
        int myRecord = 0;
        switch (record)
        {
            case "Map1Record" :
                myRecord = (int)userRankData.Map1Record;
                break;
            case "Map2Record" :
                myRecord = (int)userRankData.Map2Record;
                break;
            case "Map3Record" :
                myRecord = (int)userRankData.Map3Record;
                break;
        }
        if (myRecord == 0) return info;

        int myRank = -1;
        DataSnapshot snapshot = await _reference
            .Child("RankData")
            .OrderByChild(record)
            .EndAt(myRecord - 1)
            .GetValueAsync();
        if (snapshot != null)
        {
            myRank = (int)snapshot.ChildrenCount + 1;
        }
  
        info.Rank = myRank;
        info.Nickname = userRankData.Nickname;
        info.RecordOrScore = FormatData(myRecord);
        return info;
    }
    
    public async Task<UserRankInfo> LoadUserRank()
    {
        RankData userRankData = await LoadRankData();
        UserRankInfo info = new UserRankInfo();
        int myRecord = (int)userRankData.Score;
        
        int myRank = -1;
        DataSnapshot snapshot = await _reference
            .Child("RankData")
            .OrderByChild("Score")
            .StartAt(myRecord + 1)
            .GetValueAsync();
        
        if (snapshot != null)
        {
            myRank = (int)snapshot.ChildrenCount + 1;
        }

        info.Rank = myRank;
        info.Nickname = userRankData.Nickname;
        info.RecordOrScore = myRecord.ToString();
        
        return info;
    }
    
    #region TestCode

    private void Test()
    {
        FirebaseUser user = _auth.CurrentUser;
        // DB의 최상위 경로
        //DatabaseReference reference = FirebaseManager_LSW.Database.RootReference;
        // 최상위 경로의 값을 _text로 변경
        //reference.SetValueAsync(_text);
        
        // Root/UserData/UserID 경로
        DatabaseReference userInfo = _reference.Child("RankData").Child(user.UserId);

        // userData를 json으로 변환하여 이를 해당 경로에 저장
        string json = JsonUtility.ToJson(_rankData);
        userInfo.SetRawJsonValueAsync(json);
    }
    
    // GameManager 구현 이후 정보 읽어서 데이터 저장하는 메서드
    private void SaveRecord()
    {
        // todo : GameManager 연동
        FirebaseUser user = _auth.CurrentUser;
        // DB의 최상위 경로
        //DatabaseReference reference = FirebaseManager_LSW.Database.RootReference;
        // 최상위 경로의 값을 _text로 변경
        //reference.SetValueAsync(_text);
        
        // Root/UserID/UserData/ 경로
        DatabaseReference userInfo = _reference.Child("RankData").Child(user.UserId);

        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary["Map1Record"] = 85456;
        dictionary["Map2Record"] = 85456;
        dictionary["Map3Record"] = 85456;
        dictionary["Score"] = 13;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        stopwatch.Stop();

        long mapRecord = stopwatch.ElapsedMilliseconds;
    }

    private void Open()
    {
        _rankPanel.SetActive(true);
    }
    
    // 플레이어 Info에 쓰이는 메서드
    private async Task<RankData> LoadRankData()
    {
        FirebaseUser user = _auth.CurrentUser;
        DatabaseReference userInfo = _reference.Child("RankData").Child(user.UserId);
        RankData rankData = new RankData();

        DataSnapshot snapshot = await userInfo.GetValueAsync();
        if (snapshot.Exists)
        {
            rankData = JsonUtility.FromJson<RankData>(snapshot.GetRawJsonValue());
        }
        
        return rankData;
    }
    
    public void LoadUserRank_Legacy()
    {
        FirebaseUser user = _auth.CurrentUser;
        _reference.Child("RankData")
            .OrderByChild("Score")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted) return;

                int rank = 1;
                int myRank = 0;

                string myId = user.UserId;
                DataSnapshot snapshots = task.Result;
                foreach (var snapshot in snapshots.Children)
                {
                    RankData rankData = JsonUtility.FromJson<RankData>(snapshot.GetRawJsonValue());
                    if (snapshot.Key == myId)
                    {
                        myRank = rank;
                        break;
                    }

                    rank++;
                }
            });
    }
    
    #endregion
}
