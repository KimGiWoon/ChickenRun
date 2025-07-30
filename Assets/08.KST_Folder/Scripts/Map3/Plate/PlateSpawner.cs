using System.Collections;
using System.Collections.Generic;
using Kst;
using Photon.Pun;
using UnityEngine;

public class PlateSpawner : MonoBehaviourPunCallbacks
{
    //오브젝트 풀 관련
    // [SerializeField] private PhotonPooledObject[] _platePrefab;
    [SerializeField] private PooledObject[] _platePrefab;
    // private Dictionary<string, PhotonObjectPool> _platePools = new();
    private Dictionary<string, ObjectPool> _platePools = new();
    //스폰 관련
    [SerializeField] private float _spawnTiming = 2f;
    [SerializeField] private float _spawnX;

     

    public void StartSpawn()
    {
        Init();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(IE_Spawn());
        }
    }
    private void Init()
    {
        foreach (var prefab in _platePrefab)
        {
            if (!_platePools.ContainsKey(prefab.gameObject.name))
            {
                // if (PhotonNetwork.IsMasterClient)
                // {
                // PhotonObjectPool pool = new(transform, prefab.name, 10);
                ObjectPool pool = new(transform, prefab, 10);
                _platePools.Add(prefab.gameObject.name, pool);
                // }
            }
        }
    }

    private IEnumerator IE_Spawn()
    {
        while (true)
        {
            SpawnPlate();
            yield return new WaitForSeconds(_spawnTiming);
        }
    }

    void SpawnPlate()
    {
        int index = Random.Range(0, _platePrefab.Length);
        // PhotonPooledObject prefab = _platePrefab[index];
        var prefab = _platePrefab[index];

        float randomX = Random.Range(-_spawnX, _spawnX);
        Vector3 spawnPos = new (randomX, transform.position.y, 0);

        if (_platePools.TryGetValue(prefab.gameObject.name, out var pool))
        {
            var go = pool.PopPool();
            go.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        }

        // photonView.RPC(nameof(RPC_SyncPlate), RpcTarget.Others, prefab.gameObject.name, spawnPos);
    }

    // [PunRPC]
    // void RPC_SyncPlate(string name, Vector3 spawnPos)
    // {
    //     if (!PhotonNetwork.IsMasterClient)
    //     {
    //         var pool = _platePools[name];
    //         var go = pool.PopPool();
    //         go.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
    //     }
    // }
}