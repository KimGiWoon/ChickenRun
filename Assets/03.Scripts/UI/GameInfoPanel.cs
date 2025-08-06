using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameInfoPanel : UIBase
{
    #region Serialized fields

    [SerializeField] private List<GameObject> gameInfoPanels = new List<GameObject>();

    #endregion // Serialized fields




    #region public funcs

    public void SetShow(MapType mapType)
    {
        for (int i = 0; i < gameInfoPanels.Count; i++) {
            if (i == (int)mapType) {
                gameInfoPanels[i].SetActive(true);
            }
            else {
                gameInfoPanels[i].SetActive(false);
            }
        }
        base.SetShow();
    }

    #endregion // public funcs

}
