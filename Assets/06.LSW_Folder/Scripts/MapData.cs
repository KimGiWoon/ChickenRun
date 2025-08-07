using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData
{
    public string MapType;
    public long Record;
    public int EggCount;
    public bool IsWin;

    public MapData(string type)
    {
        MapType = type;
    }
}
