using System.Collections;
using System.Collections.Generic;
using Kst;
using Photon.Pun;
using UnityEngine;

public class PlateSpawner : MonoBehaviourPunCallbacks
{
    //오브젝트 풀 관련
    [SerializeField] private PooledObject[] _platePrefabs;
    private Dictionary<string, ObjectPool> _platePools = new();
    //스폰 관련
    [SerializeField] private float _spawnTiming = 2f;
    [SerializeField] private float _spawnX;

    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;

    /// <summary>
    /// 원판 스폰 시작하는 로직
    /// 마스터클라이언트가 담당하여 원판을 스폰하며, 스폰중일 경우 return 된다.
    /// </summary>
    public void StartSpawn()
    {
        //스폰 중일 경우 리턴
        if (_isSpawning) return;
        _isSpawning = true;

        InitPools();

        if (PhotonNetwork.IsMasterClient)
        {
            _spawnCoroutine = StartCoroutine(IE_Spawn());
        }
    }

    /// <summary>
    /// 각 Plate 프리펩에 해당하는 풀 생성
    /// </summary>
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

    /// <summary>
    /// 마스터 클라이언트가 Plate 종류와 위치 결정 후,
    /// 모든 클라이언트에게 동기화 시키는 로직
    /// </summary>
    /// <returns> spawnTiming 마다 실행 </returns>
    private IEnumerator IE_Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(_spawnTiming);

            // 마스터 클라이언트가 Plate 종류와 위치 결정

            //랜덤으로 plate 프리펩 중 하나 선택
            int index = Random.Range(0, _platePrefabs.Length);
            string plateName = _platePrefabs[index].name;
            //랜덤으로 x축 스폰 위치 결정
            float randomX = Random.Range(-_spawnX, _spawnX);
            //스폰 위치 결정
            Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0);
            //랜덤 이동 속도 설정
            float randomSpeed = Random.Range(1f, 4f);

            // 모든 클라이언트에게 동기화
            photonView.RPC(nameof(PlateSpawn), RpcTarget.All, plateName, spawnPos, randomSpeed);
        }
    }

    /// <summary>
    /// Plate 이름, 스폰 위치, 이동속도를 매개변수로 받은 후,
    /// 풀에서 꺼낸 후 해당 옵션 설정하는 로직
    /// </summary>
    /// <param name="plateName">Plate의 이름</param>
    /// <param name="spawnPos">Plate가 스폰될 위치</param>
    /// <param name="speed">Plate의 낙하 속도</param>
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
    
    /// <summary>
    /// 스폰을 멈추는 로직
    /// </summary>
    public void StopSpawn()
    {
        if (!_isSpawning) return;
        _isSpawning = false;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }
}