using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingBoardPanel : UIBase
{
    [SerializeField] private GameObject _userPrefab;
    [SerializeField] private Transform _parent;
    protected GameObjectPool _boardPool;

    private void Awake()
    {
        _boardPool = new GameObjectPool(_userPrefab,_parent,20);
    }

    public override void SetHide()
    {
        ClearBoard();
        gameObject.SetActive(false);
    }
    
    private void ClearBoard()
    {
        foreach (Transform child in _parent)
        {
            _boardPool.ReturnPool(child.gameObject);
        }
    }
}
