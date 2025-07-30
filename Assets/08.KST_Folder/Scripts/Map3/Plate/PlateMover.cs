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
        // if (PhotonNetwork.IsMasterClient)
        //     _pooled = GetComponent<PhotonPooledObject>();
        _pooled = GetComponent<PooledObject>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            _speed = Random.Range(_minspeed, _maxSpeed);
            photonView.RPC(nameof(SetSpeed), RpcTarget.AllBuffered, _speed);
        }
    }
    void Update()
    {
        transform.Translate(Vector2.down * _speed * Time.deltaTime);

        if (transform.position.y < -6f)
            photonView.RPC(nameof(RPC_ReturnPlate), RpcTarget.MasterClient);
    }

    [PunRPC]
    void SetSpeed(float speed)
    {
        _speed = speed;
    }

    [PunRPC]
    public void RPC_ReturnPlate()
    {
        if (PhotonNetwork.IsMasterClient)
            _pooled.ReturnPool();
    }
}