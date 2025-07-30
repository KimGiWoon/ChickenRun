using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class PhotonObjectPool
    {
        private Stack<PhotonPooledObject> _stack;
        private string _prefabPath;
        // private PooledObject _targetPrefab;
        private GameObject _poolObject;

        public PhotonObjectPool(Transform parent, string prefabPath, int initSize = 5) => Init(parent, prefabPath, initSize);

        private void Init(Transform parent, string prefabPath, int initSize)
        {
            _stack = new Stack<PhotonPooledObject>(initSize);
            _prefabPath = prefabPath;
            _poolObject = new GameObject($"{prefabPath} Pool");
            _poolObject.transform.parent = parent;

            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < initSize; i++)
                {
                    CreatePooledObject();
                }
            }
        }

        public PhotonPooledObject PopPool()
        {
            if (_stack.Count == 0) CreatePooledObject();

            PhotonPooledObject pooledObject = _stack.Pop();
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        public void PushPool(PhotonPooledObject target)
        {
            target.transform.parent = _poolObject.transform;
            target.gameObject.SetActive(false);
            _stack.Push(target);
        }


        private void CreatePooledObject()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // GameObject go = PhotonNetwork.Instantiate(_prefabPath, Vector3.zero, Quaternion.identity);
            GameObject go = PhotonNetwork.Instantiate(_prefabPath, Vector3.zero, Quaternion.identity);
            PhotonPooledObject obj = go.GetComponent<PhotonPooledObject>();
            obj.PooledInit(this);
            PushPool(obj);
        }
    }
}