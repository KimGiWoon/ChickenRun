using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map2RecordPanel : RankingBoardPanel
{
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadRecordRank("Map2Record", _boardPool);
    }
}
