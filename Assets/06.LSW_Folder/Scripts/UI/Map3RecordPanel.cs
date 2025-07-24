using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map3RecordPanel : RankingBoardPanel
{
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadRecordRank("Map3Record", _boardPool);
    }
}
