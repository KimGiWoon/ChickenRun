using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // 인게임 BGM
    public enum Bgms
    {
        BGM_InGame1,    // 인게임 1 BGM
        BGM_InGame2,    // 인게임 2 BGM
        BGM_InGame3,    // 인게임 3 BGM
        BGM_InGame4,    // 인게임 4 BGM
        BGM_Login,      // 로그인 BGM
        BGM_Lobby       // 로비 BGM
    }

    // 인게임 SFX
    public enum Sfxs
    {
        SFX_Jump,       // 점프
        SFX_GetEgg,     // 달걀 획득
        SFX_Goal,       // 골
        SFX_Count,      // 카운트
        SFX_Start,      // 스타트
        SFX_DropWater,  // 입수
        SFX_Walk,       // 걷기
        SFX_Shot,       // 샷
        SFX_Hit,        // 맞음
        SFX_Win,        // 이겼다
        SFX_Lose,       // 졌다
        SFX_Death,      // 죽었다
        SFX_Alarm,      // 알람
        SFX_Clear,      // 클리어
        SFX_Defeat      // 실패
    }
    public enum Sfx_Emotion
    {
        SFX_Smile = 0,  // 웃음
        SFX_Suprised,   // 놀람
        SFX_Quiz,       // 농락
        SFX_Angry,      // 분노
        SFX_Heart,      // 하트
        SFX_Crying      // 울음
    }

    [Header("BGM, SFX Sound Files")]
    [SerializeField] AudioClip[] _bgmFiles;
    [SerializeField] AudioClip[] _sfxFiles;
    [SerializeField] AudioClip[] _sfxEmotionFiles;

    [Header("Audio Source Reference")]
    [SerializeField] public AudioSource _bgmAudioSource;
    [SerializeField] public AudioSource _sfxAudioSource;

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
        // 플레이 모드 확인하여 이벤트 구독 해제
        if (Application.isPlaying)
        {
            if(SettingManager.Instance != null)
            {
                if(SettingManager.Instance.BGM != null)
                {
                    SettingManager.Instance.BGM.OnChanged -= BgmVolume;
                }

                if(SettingManager.Instance.SFX != null)
                {
                    SettingManager.Instance.SFX.OnChanged -= SfxVolume;
                }
            }
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

    // SFX 정지
    public void StopSFX()
    {
        _sfxAudioSource.Stop();
    }

    // SFX 플레이
    public void PlaySFX(Sfxs sfx)
    {
        _sfxAudioSource.PlayOneShot(_sfxFiles[(int)sfx]);
    }
    // 이모티콘 SFX 플레이
    public void PlaySFX(int index)
    {
        _sfxAudioSource.PlayOneShot(_sfxEmotionFiles[index]);
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
