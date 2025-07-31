using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager_Map2 : MonoBehaviour
{
    private static AudioManager_Map2 _instance;
    public static AudioManager_Map2 Instance
    {
        get
        {
            return _instance;
        }
    }
    
    // 인게임 BGM
    public enum Bgms
    {
        BGM_InGame
    }

    // 인게임 SFX
    public enum Sfxs
    {
        SFX_Jump,
        SFX_GetEgg,
        SFX_Goal,
        SFX_Count,
        SFX_Start,
        SFX_DropWater
    }

    [Header("BGM, SFX Sound Files")]
    [SerializeField] AudioClip[] _bgmFiles;
    [SerializeField] AudioClip[] _sfxFiles;

    [Header("Audio Source Reference")]
    [SerializeField] public AudioSource _bgmAudioSource;
    [SerializeField] public AudioSource _sfxAudioSource;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else 
        { 
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        BgmUpdate(SettingManager.Instance.BGM.Value);
        SfxUpdate(SettingManager.Instance.SFX.Value);

        SettingManager.Instance.BGM.OnChanged += BgmUpdate;
        SettingManager.Instance.SFX.OnChanged += SfxUpdate;
    }

    public void OnDestroy()
    {
        if (Application.isPlaying)
        {
            SettingManager.Instance.BGM.OnChanged -= BgmUpdate;
            SettingManager.Instance.SFX.OnChanged -= SfxUpdate;
        }
    }

    // BGM 플레이
    public void PlayBGM(Bgms bgm)
    {
        if (_bgmFiles.Length > (int)bgm)
        {
            _bgmAudioSource.clip = _bgmFiles[(int)bgm];
            _bgmAudioSource.loop = true;
            _bgmAudioSource.Play();
        }
    }

    // BGM 정지
    public void StopBGM()
    {
        _bgmAudioSource.Stop();
    }

    // SFX 플레이
    public void PlaySFX(Sfxs sfx)
    {
        if (_sfxFiles.Length > (int)sfx)
        {
            _sfxAudioSource.PlayOneShot(_sfxFiles[(int)sfx]);
        }
    }

    // BGM 볼륨
    private void BgmUpdate(float volume)
    {
        _bgmAudioSource.volume = volume;
    }

    // SFX 볼륨
    private void SfxUpdate(float volume)
    {
        _sfxAudioSource.volume = volume;
    }
}
