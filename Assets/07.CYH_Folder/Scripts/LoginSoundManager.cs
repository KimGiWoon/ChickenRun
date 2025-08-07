using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginSoundManager : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM(SoundManager.Bgms.BGM_Login);
    }
}
