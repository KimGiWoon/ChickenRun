using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class ObjectPool
    {
        private Stack<PooledObject> _stack;
        private string _prefabPath;
        // private PooledObject _targetPrefab;
        private GameObject _poolObject;

        public ObjectPool(Transform parent, string prefabPath, int initSize = 5) => Init(parent, prefabPath, initSize);

        private void Init(Transform parent, string prefabPath, int initSize)
        {
            _stack = new Stack<PooledObject>(initSize);
            _prefabPath = prefabPath;
            _poolObject = new GameObject($"{prefabPath} Pool");
            _poolObject.transform.parent = parent;

            for (int i = 0; i < initSize; i++)
            {
                CreatePooledObject();
            }
        }

        public PooledObject PopPool()
        {
            if (_stack.Count == 0) CreatePooledObject();

            PooledObject pooledObject = _stack.Pop();
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        public void PushPool(PooledObject target)
        {
            target.transform.parent = _poolObject.transform;
            target.gameObject.SetActive(false);
            _stack.Push(target);
        }


        private void CreatePooledObject()
        {
            GameObject go = PhotonNetwork.Instantiate(_prefabPath, Vector3.zero, Quaternion.identity);
            PooledObject obj = go.GetComponent<PooledObject>();
            obj.PooledInit(this);
            PushPool(obj);
        }
    }
}