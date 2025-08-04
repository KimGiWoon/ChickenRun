using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // 인게임 BGM
    public enum Bgms
    {
        BGM_InGame1,
        BGM_InGame2,
        BGM_InGame3,
        BGM_InGame4,
        BGM_Login,
        BGM_Lobby
    }

    // 인게임 SFX
    public enum Sfxs
    {
        SFX_Jump,
        SFX_GetEgg,
        SFX_Goal,
        SFX_Count,
        SFX_Start,
        SFX_DropWater,
        SFX_Walk,
        SFX_Shot,
        SFX_Hit,
        SFX_Win,
        SFX_Lose,
        SFX_Smile, //웃음
        SFX_Suprised, // 놀람
        SFX_Quiz, //농락
        SFX_Angry,//분노
        SFX_Heart, //하트
        SFX_Crying //울음
    }

    [Header("BGM, SFX Sound Files")]
    [SerializeField] AudioClip[] _bgmFiles;
    [SerializeField] AudioClip[] _sfxFiles;

    [Header("Audio Source Reference")]
    [SerializeField] public AudioSource _bgmAudioSource;
    [SerializeField] public AudioSource _sfxAudioSource;
    [SerializeField] public AudioSource _loopSfxAS;

    private void Start()
    {
        BgmVolume(SettingManager.Instance.BGM.Value);
        SfxVolume(SettingManager.Instance.SFX.Value);

        // 이벤트 구독
        SettingManager.Instance.BGM.OnChanged += BgmVolume;
        SettingManager.Instance.SFX.OnChanged += SfxVolume;
    }

    // 게임 종료 시 
    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 플레이 모드 확인하여 이벤트 구독 해제
        if (Application.isPlaying)
        {
            SettingManager.Instance.BGM.OnChanged -= BgmVolume;
            SettingManager.Instance.SFX.OnChanged -= SfxVolume;
        }
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

    //Loop SFX 플레이
    public void PlayLoopSFX(Sfxs sfx)
    {
        if (_loopSfxAS.isPlaying && _loopSfxAS.clip == _sfxFiles[(int)sfx]) return;

        _loopSfxAS.clip = _sfxFiles[(int)sfx];
        _loopSfxAS.loop = true;
        _loopSfxAS.Play();
    }
    public void StopLoopSFX()
    {
        _loopSfxAS.Stop();
        _loopSfxAS.clip = null;
    }


    // BGM 볼륨
    public void BgmVolume(float volume)
    {
        _bgmAudioSource.volume = volume;
    }

    // SFX 볼륨
    public void SfxVolume(float volume)
    {
        _sfxAudioSource.volume = volume;
    }
}
