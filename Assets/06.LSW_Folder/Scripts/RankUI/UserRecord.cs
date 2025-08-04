using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 랭킹보드 하단에 표시되는 유저 개인 랭킹
public class UserPersonalRecord : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private TextMeshProUGUI _scoreOrRecord;

    [SerializeField] private Button _btn; 
    private string _uid;

    public static event Action<string> OnClickPlayerInfo;
    
    private void Awake()
    {
        _btn.onClick.AddListener( () => OnClick());
    }
    
    // MVC 패턴 적용을 위한 Public 메서드
    public void SetRecordText(int rank, string nickname, string record, string uid)
    {
        _rank.text = $"{rank}";
        _nickname.text = nickname;
        _scoreOrRecord.text = record;
        _uid = uid;
    }
    
    // MVC 패턴 적용을 위한 Public 메서드
    // score의 변수형을 string으로 바꾸고 위의 메서드를 합쳐도 될 것 같음
    public void SetScoreText(int rank, string nickname, long score, string uid)
    {
        _rank.text = $"{rank}";
        _nickname.text = nickname;
        _scoreOrRecord.text = $"{score}";
        _uid = uid;
    }

    private void OnClick()
    {
        OnClickPlayerInfo?.Invoke(_uid);
    }
}
