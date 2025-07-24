using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePanel : RankingBoardPanel
{
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadScoreRank(_boardPool);
    }
}
