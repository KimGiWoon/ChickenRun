using Photon.Pun;
using UnityEngine;

public class PlateMover : MonoBehaviourPun
{
    [SerializeField] private float _minspeed = 1f;
    [SerializeField] private float _maxSpeed = 4f;
    private float _speed;

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

        //TODO <김승태> : 추후 오브젝트 풀로 관리해야함
        if (transform.position.y < -6f)
        {
            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void SetSpeed(float speed)
    {
        _speed = speed;
    }
}