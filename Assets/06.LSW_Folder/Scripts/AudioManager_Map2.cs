using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager_Map2 : Singleton<AudioManager_Map2>
{
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

    private void Start()
    {
        BgmUpdate(SettingManager.Instance.BGM.Value);
        SfxUpdate(SettingManager.Instance.SFX.Value);

        SettingManager.Instance.BGM.OnChanged += BgmUpdate;
        SettingManager.Instance.SFX.OnChanged += SfxUpdate;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        SettingManager.Instance.BGM.OnChanged -= BgmUpdate;
        SettingManager.Instance.SFX.OnChanged -= SfxUpdate;
    }

    // BGM 플레이
    public void PlayBGM(Bgms bgm)
    {
        _bgmAudioSource.clip = _bgmFiles[(int)bgm];
        _bgmAudioSource.loop = true;
        _bgmAudioSource.Play();
    }

    // BGM 정지
    public void StopBGM()
    {
        _bgmAudioSource.Stop();
    }

    // SFX 플레이
    public void PlaySFX(Sfxs sfx)
    {
        _sfxAudioSource.PlayOneShot(_sfxFiles[(int)sfx]);
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
