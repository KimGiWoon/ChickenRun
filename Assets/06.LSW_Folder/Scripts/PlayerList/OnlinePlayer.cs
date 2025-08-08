using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnlinePlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nickname;

    public void SetText(string nickname)
    {
        _nickname.text = nickname;
    }
}
