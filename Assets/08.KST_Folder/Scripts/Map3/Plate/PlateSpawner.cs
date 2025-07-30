using System.Collections;
using System.Collections.Generic;
using Kst;
using Photon.Pun;
using UnityEngine;

public class PlateSpawner : MonoBehaviourPunCallbacks
{
    //오브젝트 풀 관련
    // [SerializeField] private PhotonPooledObject[] _platePrefab;
    [SerializeField] private PooledObject[] _platePrefabs;
    // private Dictionary<string, PhotonObjectPool> _platePools = new();
    private Dictionary<string, ObjectPool> _platePools = new();
    //스폰 관련
    [SerializeField] private float _spawnTiming = 2f;
    [SerializeField] private float _spawnX;

    public void StartSpawn()
    {
        InitPools();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(IE_Spawn());
        }
    }

    private void InitPools()
    {
        foreach (var prefab in _platePrefabs)
        {
            if (!_platePools.ContainsKey(prefab.name))
            {
                var pool = new ObjectPool(transform, prefab, 10);
                _platePools.Add(prefab.name, pool);
            }
        }
    }

    private IEnumerator IE_Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnTiming);

            // 마스터 클라이언트가 Plate 종류와 위치 결정
            int index = Random.Range(0, _platePrefabs.Length);
            string plateName = _platePrefabs[index].name;
            float randomX = Random.Range(-_spawnX, _spawnX);
            Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0);
            float randomSpeed = Random.Range(1f, 4f);

            // 모든 클라이언트에게 동기화
            photonView.RPC(nameof(PlateSpawn), RpcTarget.All, plateName, spawnPos, randomSpeed);
        }
    }

    [PunRPC]
    private void PlateSpawn(string plateName, Vector3 spawnPos, float speed)
    {
        if (_platePools.TryGetValue(plateName, out var pool))
        {
            var plate = pool.PopPool();
            plate.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
            if (plate.TryGetComponent(out PlateMover mover))
                mover.SetSpeed(speed);
        }
    }
}