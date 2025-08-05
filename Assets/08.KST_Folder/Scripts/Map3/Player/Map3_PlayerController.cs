using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class Map3_PlayerController : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private PhotonView _pv;
        [SerializeField] Animator _animator;
        [SerializeField] private float _movespeed = 5f;
        private float _moveDir = 0f;

        [SerializeField] SpriteRenderer _playerRenderer;
        // 애니메이션
        public readonly int Idle_Hash = Animator.StringToHash("Idle");
        public readonly int Move_Hash = Animator.StringToHash("Move");

        int _currentAnimatorHash;
        int _reciveAnimatorHash;

        //매니저
        UIManager_Map3 _gameUIManager;
        GameManager_Map3 _gameManager;

        private Coroutine _stepCoroutine;
        private bool _isWalking = false;
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

                _isWalking = false;

                if (_stepCoroutine != null)
                {
                    StopCoroutine(_stepCoroutine);
                    _stepCoroutine = null;
                }

            }
            else if (_rb.velocity.x > 0.1f || _rb.velocity.x < -0.1f)
            {
                _currentAnimatorHash = Move_Hash;
                _animator.Play(Move_Hash);
                if (!_isWalking)
                {
                    _isWalking = true;
                    if (_stepCoroutine == null)
                        _stepCoroutine = StartCoroutine(IE_PlayerStep());
                }

            }
        }
        IEnumerator IE_PlayerStep()
        {
            while (_isWalking)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Walk);
                yield return new WaitForSeconds(0.5f);
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

        // 포톤 네트워크로 플레이어 생성 시 호출
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {

            // 오브젝트 배열을 저장
            var data = info.photonView.InstantiationData;

            // 데이터가 Null이 아니고, 데이터가 최소 하나 이상 있는지 확인
            if (data != null && data.Length > 0)
            {
                // 첫번째 데이터 스킨
                string skinName = data[0].ToString();

                Debug.Log($"적용 스킨 : {skinName}");
                ApplySkin(skinName);
            }
        }

        // 스킨 적용
        private void ApplySkin(string skinName)
        {
            // 스킨의 스프라이트 불러오기
            Sprite skinSprite = Resources.Load<Sprite>($"Sprites/Skins/{skinName}");
            if (skinSprite != null)
            {
                Debug.Log($"적용 완료 : {skinSprite}");

                // 스킨 적용
                _playerRenderer.sprite = skinSprite;

                // Common의 enum 스킨 타입 확인
                if (Enum.TryParse<SkinType>(skinName, out SkinType skinType))
                {
                    Debug.Log($"스킨 컨트롤러 타입은 {skinType}");

                    // 애니메이션 컨트롤러 변수 선언 및 Null로 초기화
                    RuntimeAnimatorController controller = null;

                    // 스킨의 애니메이션 컨트롤러 불러오기
                    switch (skinType)
                    {
                        case SkinType.Default:
                            Debug.Log("닭 스킨입니다.");
                            controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/ChickenAnimation/ChickenAnimation");
                            break;
                        case SkinType.OwletMonster:
                            Debug.Log("올빼미 스킨입니다.");
                            controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/OwletMonsterAnimation/OwletMonsterAnimatorController");
                            break;
                        case SkinType.Pig:
                            Debug.Log("돼지 스킨입니다.");
                            controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/PigAnimation/PigAnimatorController");
                            break;
                        case SkinType.PinkMonster:
                            Debug.Log("핑크몬스터 스킨입니다.");
                            controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/PinkMonsterAnimation/PinkMonsterAnimatorController");
                            break;
                        case SkinType.DudeMonster:
                            Debug.Log("파란몬스터 스킨입니다.");
                            controller = Resources.Load<RuntimeAnimatorController>("Sprites/Animations/DudeMonsterAnimation/DudeMonsterAnimatorController");
                            break;
                        default:
                            break;
                    }

                    // 컨트롤러가 Null이 아니라면 스킨에 맞는 컨트롤러 체인지
                    if (controller != null)
                    {
                        _animator.runtimeAnimatorController = controller;
                    }
                }

                Debug.Log("적용이 되었습니다.");
            }
            else
            {
                Debug.LogWarning($"[PlayerController] 스킨 적용 실패: {skinName}");
            }
        }
    }
}