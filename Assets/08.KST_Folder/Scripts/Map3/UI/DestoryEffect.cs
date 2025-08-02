using Kst;
using UnityEngine;

public class DestoryEffect : MonoBehaviour
{
    [SerializeField] private PooledObject _pooledObj;
    [SerializeField] private Animator _animator;
    public readonly int Idle_Hash = Animator.StringToHash("Destory");

    void OnEnable() => _animator.Play(Idle_Hash);
    public void EffectReturnPool() => _pooledObj.ReturnPool();
}