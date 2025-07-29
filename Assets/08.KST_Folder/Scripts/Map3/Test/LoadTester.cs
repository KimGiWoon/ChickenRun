using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadTester : MonoBehaviour
{
    [SerializeField] Button btn;

    void Start()
    {
        btn.onClick.AddListener(() => SceneManager.LoadScene("GameScene_Map3"));
    }
}
