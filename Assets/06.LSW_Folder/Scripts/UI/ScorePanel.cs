using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePanel : RankingBoardPanel
{
    // 랭킹보드 업데이트 메서드 -> DatabaseManager에 구현
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadScoreRank(_boardPool);
    }
}
