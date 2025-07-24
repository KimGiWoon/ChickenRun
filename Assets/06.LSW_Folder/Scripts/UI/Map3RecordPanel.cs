using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map3RecordPanel : RankingBoardPanel
{
    // 랭킹보드 업데이트 메서드 -> DatabaseManager에 구현
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadRecordRank("Map3Record", _boardPool);
    }
}
