using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Map3_PlayerController : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private PhotonView _pv;
        [SerializeField] Animator _animator;
        [SerializeField] private float _movespeed = 5f;
        private float _moveDir = 0f;

        [SerializeField] SpriteRenderer _playerRenderer;
        // 애니메이션
        public readonly int Idle_Hash = Animator.StringToHash("ChickenIdle");
        public readonly int Move_Hash = Animator.StringToHash("ChickenMove");

        int _currentAnimatorHash;
        int _reciveAnimatorHash;

        //매니저
        UIManager_Map3 _gameUIManager;
        GameManager_Map3 _gameManager;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            ManagerInit();

            // 조종 가능 유/무에 따른 레이어 설정 (충돌 관련 세팅)
            if (photonView.IsMine)
            {
                gameObject.layer = LayerMask.NameToLayer("Player");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("RemotePlayer");
                // 반 투명화
                TranslucentSetting();
            }
        }
        void OnEnable()
        {
            _gameUIManager.OnGameStart += GetComponent<PlayerShooter>().SetCanAttack;
        }
        void OnDisable()
        {
            _gameUIManager.OnGameStart -= GetComponent<PlayerShooter>().SetCanAttack;

        }
        void Start()
        {
            if (!photonView.IsMine) return;

            _gameManager._gameUIManager.SetPlayerPosition(transform);
        }
        void Update()
        {
            if (!_pv.IsMine) return;
            PlayerStateUpdate();
        }

        void FixedUpdate()
        {
            if (!_pv.IsMine) return;

            _rb.velocity = new Vector2(_moveDir * _movespeed, _rb.velocity.y);
        }
        public void SetDir(int dir)
        {
            if (!_pv.IsMine) return;
            _moveDir = dir;
            if (dir == 1)
                _playerRenderer.flipX = false;
            else if (dir == -1)
                _playerRenderer.flipX = true;
        }

        private void ManagerInit()
        {
            if (_gameUIManager == null)
            {
                _gameUIManager = FindObjectOfType<UIManager_Map3>();
            }

            if (_gameManager == null)
            {
                _gameManager = FindObjectOfType<GameManager_Map3>();
            }
        }
        // 자기 자신을 제외한 플레이어 반 투명화 세팅
        private void TranslucentSetting()
        {
            Color color = _playerRenderer.color;
            color.a = 0.5f;
            _playerRenderer.color = color;
        }

        private void PlayerStateUpdate()
        {
            if (_rb.velocity == Vector2.zero)
            {
                _currentAnimatorHash = Idle_Hash;
                _animator.Play(Idle_Hash);

                SoundManager.Instance.StopLoopSFX();
            }
            else if (_rb.velocity.x > 0.1f || _rb.velocity.x < -0.1f)
            {
                _currentAnimatorHash = Move_Hash;
                //TODO <김승태> : SFX 변경 필요
                _animator.Play(Move_Hash);
                // 루프용 SFX로 변경 필요. -> SoundManager의 구조 변화 필요.
                // SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Jump);
                SoundManager.Instance.PlayLoopSFX(SoundManager.Sfxs.SFX_Walk);
            }
        }

        // 플레이어 포톤 뷰 동기화
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 동기화 데이터 보내기
            if (stream.IsWriting)
            {
                stream.SendNext(_currentAnimatorHash);  // 현재 플레이어의 애니메이션 데이터 전송
                stream.SendNext(_playerRenderer.flipX); // 플레이어의 방향 데이터 전송
            }
            else    // 동기화 데이터 받기
            {
                _reciveAnimatorHash = (int)stream.ReceiveNext();    // 애니메이션 정보 받기
                _playerRenderer.flipX = (bool)stream.ReceiveNext();
                _animator.Play(_reciveAnimatorHash);   // 받은 정보로 애니메이션 플레이
            }
        }
    }
}