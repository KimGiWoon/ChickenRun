using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlateSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] _platePrefab;
    [SerializeField] private float _spawnTiming = 2f;
    [SerializeField] private float _spawnX;

    // void Start()
    // {
        // StartSpawn();
    // }
    public void StartSpawn()
    {
        //마스터 클라이언트만 생성할 수 있도록
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(IE_Spawn());
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
        GameObject go = _platePrefab[index];

        float randomX = Random.Range(-_spawnX, _spawnX);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0);

        PhotonNetwork.Instantiate(go.name, spawnPos, Quaternion.identity);
        Debug.Log("스폰");
    }
}