using Kst;
using UnityEngine;

public class DestoryEffect : MonoBehaviour
{
    [SerializeField] private PooledObject _pooledObj;
    [SerializeField] private Animator _animator;
    public readonly int Idle_Hash = Animator.StringToHash("Destory");

    /// <summary>
    /// 풀에서 꺼내질 때(해당 오브젝트 활성화 시) 애니메이션 실행
    /// </summary>
    void OnEnable() => _animator.Play(Idle_Hash);

    /// <summary>
    /// Destory 애니메이션 끝 단에 해당 메서드를 호출하여 풀 반납
    /// </summary>
    public void EffectReturnPool() => _pooledObj.ReturnPool();
}