using System.Collections;
using System.Collections.Generic;
using Kst;
using UnityEngine;
using UnityEngine.UI;

public class EggTester : MonoBehaviour
{
    [SerializeField] Button btn;

    void Start()
    {
        btn.onClick.AddListener(() => EggManager.Instance.GainEgg(1));
    }
}
