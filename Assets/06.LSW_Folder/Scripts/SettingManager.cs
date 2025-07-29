using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : Singleton<SettingManager>
{
    public Property<float> BGM;
    public Property<float> SFX;
    public Property<bool> CamMode;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        BGM = new Property<float>(0.5f);
        SFX = new Property<float>(0.5f);
    }

    public void SetBGM(float input)
    {
        BGM.Value = input;
    }
    
    public void SetSFX(float input)
    {
        SFX.Value = input;
    }

    public void SetCamMode(bool input)
    {
        CamMode.Value = input;
    }
}
