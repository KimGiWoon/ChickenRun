using Kst;
using Photon.Pun;
using UnityEngine;

public class PlateMover : MonoBehaviourPun
{
    [SerializeField] private float _minspeed = 1f;
    [SerializeField] private float _maxSpeed = 4f;
    private float _speed;
    // private PhotonPooledObject _pooled;
    private PooledObject _pooled;
    void Awake()
    {
        _pooled = GetComponent<PooledObject>();
    }
    void Update()
    {
        transform.Translate(Vector2.down * _speed * Time.deltaTime);

        if (transform.position.y < -6f)
            _pooled.ReturnPool();
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }
    public void ReturnPool() => _pooled.ReturnPool();

    [PunRPC]
    public void RequestReturn()
    {
        _pooled.ReturnPool();
    }
}