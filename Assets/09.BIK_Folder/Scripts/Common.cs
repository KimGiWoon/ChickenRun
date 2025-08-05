using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Common
{
    public static Color ConvertColorTypeToUnityColor(ColorType type)
    {
        return type switch {
            ColorType.White => Color.white,
            ColorType.Red => Color.red,
            ColorType.Blue => Color.blue,
            ColorType.Green => Color.green,
            ColorType.Yellow => Color.yellow,
            ColorType.Purple => new Color(0.5f, 0f, 0.5f),
            ColorType.Orange => new Color(1f, 0.5f, 0f),
            ColorType.Pink => new Color(1f, 0.4f, 0.7f),
            ColorType.Cyan => Color.cyan,
            _ => Color.white,
        };
    }

    public static string ConvertColorTypeToHex(ColorType type)
    {
        Color color = ConvertColorTypeToUnityColor(type);
        return ColorUtility.ToHtmlStringRGBA(color);
    }
}



#region Type Def

public enum UIType
{
    PlayBase,
    PlayMakeRoom,
    PlayFindRoom,
    PlayLobby,
}

public enum MapType
{
    Map1 = 0,
    Map2,
    Map3,
    Map4,
}

public enum ColorType
{
    White = 0,
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange,
    Pink,
    Cyan,
}

// 스킨 타입
public enum  SkinType
{
    Default = 0,
    OwletMonster,
    Pig,
    PinkMonster
}

#endregion // Type Def