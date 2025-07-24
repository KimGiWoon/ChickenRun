using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserPersonalRecord : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _nickname;
    [SerializeField] private TextMeshProUGUI _scoreOrRecord;

    public void SetRecordText(int rank, string nickname, string record)
    {
        _rank.text = $"{rank}";
        _nickname.text = nickname;
        _scoreOrRecord.text = record;
    }
    
    public void SetScoreText(int rank, string nickname, long score)
    {
        _rank.text = $"{rank}";
        _nickname.text = nickname;
        _scoreOrRecord.text = $"{score}";
    }
}
