using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 랭킹보드의 개인 랭킹을 관리할 오브젝트 풀
public class GameObjectPool
{
    private List<GameObject> _pool;
    private GameObject _prefab;
    private Transform _parent;
    private int _currentIndex;

    // 오브젝트 풀 생성자, volume은 생성할 때 입력
    public GameObjectPool(GameObject prefab, Transform parent, int volume)
    {
        _pool = new List<GameObject>();
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < volume; i++)
        {
            GameObject go = GameObject.Instantiate(_prefab, _parent);
            go.SetActive(false);
            _pool.Add(go);
        }
    }

    public GameObject GetPool()
    {
        if (_pool.Count > 0)
        {
            GameObject go = _pool[_currentIndex];
            go.SetActive(true);
            _currentIndex ++;
            return go;
        }

        else
        {
            GameObject go = GameObject.Instantiate(_prefab, _parent);
            go.SetActive(true);
            return go;
        }
    }

    public void ReturnPool(GameObject go)
    {
        go.SetActive(false);
        _currentIndex = 0;
    }
}
