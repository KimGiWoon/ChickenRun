using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private GameObject _prefab;
    private Queue<GameObject> _pool;
    private Transform _parent;

    public GameObjectPool(GameObject prefab, Transform parent, int volume)
    {
        _pool = new Queue<GameObject>();
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < volume; i++)
        {
            GameObject go = GameObject.Instantiate(_prefab, _parent);
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }

    public GameObject GetPool()
    {
        if (_pool.Count > 0)
        {
            GameObject go = _pool.Dequeue();
            go.SetActive(true);
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
        _pool.Enqueue(go);
    }
}
