using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map1RecordPanel : RankingBoardPanel
{
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadRecordRank("Map1Record", _boardPool);
    }
}
