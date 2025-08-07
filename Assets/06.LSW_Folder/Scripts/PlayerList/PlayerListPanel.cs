using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListPanel : UIBase
{
    // 패널에 보여질 닉네임, 활동 프리팹
    [SerializeField] private GameObject _userPrefab;
    // 프리팹을 생성시킬 위치
    [SerializeField] private Transform _parent;
    // 오브젝트 풀로 구현된 Scroll View
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
    
    // 랭킹보드가 바뀔 때 다시 풀로 돌아가도록 구현
    private void ClearBoard()
    {
        foreach (Transform child in _parent)
        {
            _boardPool.ReturnPool(child.gameObject);
        }
    }
    
    public override void RefreshUI()
    {
        Database_RecordManager.Instance.LoadOnlinePlayer(_boardPool);
    }
}
