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
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Database_RecordManager : Singleton<Database_RecordManager>
{
    // 테스트 용 Btn,Panel -> 연동 이후 삭제
    [SerializeField] private Button _rankBtn;
    [SerializeField] private Button _testBtn;
    [SerializeField] private Button _infoBtn;
    [SerializeField] private GameObject _rankPanel;
    [SerializeField] private GameObject _infoPanel;
    
    private FirebaseAuth _auth;
    private DatabaseReference _reference;
    
    private RankData _rankData;
    
    // DTO (Data Transfer Object) 
    // DB에 저장되는 데이터중 Rank 데이터만 필드로 가지고 있는 클래스
    [System.Serializable]
    public class RankData
    {
        public string Nickname;
        public long Map1Record;
        public long Map2Record;
        public long Map3Record;
        public long Score;
    }
    
    // DTO (Data Transfer Object) 
    // 랭킹보드에 표시될 데이터만 필드로 가지고 있는 클래스
    public class UserRankInfo
    {
        public int Rank;
        public string Nickname;
        public string RecordOrScore;
    }
    
    // 연동 이후 삭제
    protected override void Awake()
    {
        base.Awake();
        _rankBtn.onClick.AddListener(Rank);
        _testBtn.onClick.AddListener(Test);
        _infoBtn.onClick.AddListener(Info);
    }
    
    // 테스트용 자동 로그인 코드
    // 진짜 이메일/비번 아님. 아마도.
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
            }
        });
        
        // todo : GameManager 이벤트 메서드 연동
        //GameManager.Instance.OnEndGame += (data) => SaveUserRecord(data);
    }

    // GameManager 게임 종료 이벤트에 등록할 메서드
    // 플레이한 맵의 기록을 저장/갱신하고, 1등인 경우 승수를 +1한다.
    /*private void SaveUserRecord(GameResultData data)
    {
        string uid = _auth.CurrentUser.UserId;
        string key = data.MapType;
        DatabaseReference recordRef = _reference.Child("RankData").Child(uid).Child(key);
        
        // 맵 기록 저장/갱신 Transaction
        recordRef.RunTransaction(mutableData =>
        {
            int currentRecord = mutableData.Value == null ? int.MaxValue : (int)mutableData.Value;
            if((int)data.Record < currentRecord)
            {
                mutableData.Value = data.Record;
            }
            return TransactionResult.Success(mutableData);
        });
        
        // 1등했을 때 승수 갱신 Transaction
        if(data.IsWin)
        {
            DatabaseReference scoreRef = _reference.Child("RankData").Child(uid).Child("Score");
            scoreRef.RunTransaction(mutableData =>
            {
                int currentRecord = mutableData.Value == null ? 0 : (int)mutableData.Value;
                mutableData.Value = currentRecord + 1;
                return TransactionResult.Success(mutableData);
            });
        }
    }*/
    
    // 기록을 어떤 방식으로 저장하는 것이 비용이 가장 적게 들어가는가?
    // MM:SS:SS 를 그대로 캐싱하는 것은 아무리 생각해도 비용이 높을 것 같음
    // 밀리초(ms)를 사용할거면 차라리 ms를 정수값으로 저장하고
    // 이를 포맷을 바꿔서 UI에 노출시키는게 가장 낫지 않을까
    // 밀리초(int)를 MM:SS:SS 형식으로 바꿔주는 메서드
    public string FormatData(int ms)
    {
        int minute = ms / 60000;
        int second = (ms % 60000) / 1000;
        int millisecond = ms % 1000 / 10;
        return $"{minute:D2}:{second:D2}:{millisecond:D2}";
    }
    
    // Map Record에 따라 표시될 Record 랭킹보드에 사용되는 메서드
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

                    if (recordValue != 0)
                    {
                        GameObject board = boardPool.GetPool();
                        board.GetComponent<UserPersonalRecord>().SetRecordText(rank,rankData.Nickname,FormatData(recordValue));
                        rank++;
                    }
                }
            });
    }
    
    // Score에 따라 표시될 Score 랭킹보드에 사용되는 메서드
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
    
    // 유저의 데이터에 따라 랭킹을 가져오는 메서드
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
    
    // 유저의 데이터에 따라 랭킹을 가져오는 메서드(함수 오버로드)
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
    
    // PlayerInfoUI, RankUI에 쓰이는 메서드로
    // 유저 개인 랭킹 정보를 불러오는 메서드(동기 처리)
    public async Task<RankData> LoadRankData()
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
    
    #region TestCode

    // DB에 데이터 쓰기 테스트 코드
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
    // PlayerInfo UI 작업 들어갈 때 확장
    private void SaveRecord()
    {
        // todo : GameManager 연동
        FirebaseUser user = _auth.CurrentUser;
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

    // MainScene 연동 이전 임시 로그인으로 작성한 코드 -> 연동 이후 삭제
    private void Rank()
    {
        _rankPanel.SetActive(!_rankPanel.activeSelf);
    }

    // RoomScene 연동 이전 임시 버튼 연동 코드 -> 연동 이후 삭제
    private void Info()
    {
        _infoPanel.SetActive(!_infoPanel.activeSelf);
    }
    
    // 초기 개인 랭킹 표시 메서드
    // 코드가 더 복잡한 것 같아서 위 메서드로 재구현
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
