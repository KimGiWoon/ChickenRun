using Kst;
using UnityEngine;
public class EffectPoolManager : MonoBehaviour
{
    [SerializeField] PooledObject _destoryEffectPrefab;
    private ObjectPool _effectPool;

    void Start() => _effectPool = new(null, _destoryEffectPrefab, 10);

    public PooledObject GetPool() => _effectPool.PopPool();
}
