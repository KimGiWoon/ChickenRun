using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingBoardPanel : UIBase
{
    // 랭킹보드에 보여질 유저 랭킹 프리팹
    [SerializeField] private GameObject _userPrefab;
    // 랭킹보드 reference -> 프리팹을 생성시킬 위치
    [SerializeField] private Transform _parent;
    // 오브젝트 풀로 구현된 보드
    protected GameObjectPool _boardPool;

    // 오브젝트 풀 초기화 메서드
    private void Awake()
    {
        _boardPool = new GameObjectPool(_userPrefab,_parent,20);
    }

    // 오브젝트 풀 반환을 위한 override
    public override void SetHide()
    {
        ClearBoard();
        gameObject.SetActive(false);
    }
    
    // 랭킹보드가 바뀔 때 다시 풀로 돌아가도록 구현
    private void ClearBoard()
    {
        foreach (Transform child in _parent)
        {
            _boardPool.ReturnPool(child.gameObject);
        }
    }
}
