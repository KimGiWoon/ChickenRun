using Kst;
using UnityEngine;

public class PlateMover : MonoBehaviour
{
    private float _speed;
    private PooledObject _pooled;
    void Awake() => _pooled = GetComponent<PooledObject>();
    void Update()
    {
        transform.Translate(Vector2.down * _speed * Time.deltaTime);

        if (transform.position.y < -6f)
            _pooled.ReturnPool();
    }

    public void SetSpeed(float speed) => _speed = speed;
    public void ReturnPool() => _pooled.ReturnPool();
}