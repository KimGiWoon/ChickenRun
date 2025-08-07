using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BoostController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Boost Setting")]
    [SerializeField] float _boostPower = 3f;
    [SerializeField] float _boostTime = 15f;
    [SerializeField] PlayerController_Map1 _playerController;
    [SerializeField] Animator _dashAni;

    public bool _hasBoostItem = false;
    bool _isBoost = false;
    int _currentAnimatorHash;
    int _reciveAnimatorHash;
    Coroutine _boostCoroutine;

    public readonly int DashIdle_Hash = Animator.StringToHash("DashIdle");
    public readonly int DashCharge_Hash = Animator.StringToHash("DashCharge");
    public readonly int DashRun_Hash = Animator.StringToHash("DashRun");

    // 부스트 아이템 획득
    public void GetBoostItem()
    {
        if (!_hasBoostItem && photonView.IsMine)
        {
            _hasBoostItem = true;

            // 부스트 버튼 활성화
            _playerController._gameUIManager._boostButton.gameObject.SetActive(true);
        }
    }

    // 부스트 사용
    public void UseBoost()
    {
        if (!_hasBoostItem || _isBoost) return;

        if(photonView.IsMine)
        {
            _hasBoostItem = false;
            _isBoost = true;

            // 부스트 버튼 비활성화
            _playerController._gameUIManager._boostButton.gameObject.SetActive(false);

            if (_boostCoroutine != null)
            {
                StopCoroutine(_boostCoroutine);
                _boostCoroutine = null;
            }

            // 부스트 코루틴 시작
            _boostCoroutine = StartCoroutine(BoostRoutine());
        }        
    }

    // 플레이어 포톤 뷰 동기화
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 동기화 데이터 보내기
        if (stream.IsWriting)
        {
            stream.SendNext(_currentAnimatorHash);  // 현재 플레이어의 애니메이션 데이터 전송
        }
        else    // 동기화 데이터 받기
        {
            _reciveAnimatorHash = (int)stream.ReceiveNext();    // 애니메이션 정보 받기
            _dashAni.Play(_reciveAnimatorHash); // 받은 정보로 애니메이션 플레이
        }
    }

    // 부스트 루틴
    private IEnumerator BoostRoutine()
    {
        float timer = 0f;
        Vector2 direction = Vector2.right;
        _playerController._playerRigid.velocity = Vector2.zero;

        // 대쉬 충전 애니메이션
        _currentAnimatorHash = DashCharge_Hash;
        _dashAni.Play(DashCharge_Hash);

        yield return new WaitForSeconds(1.5f);

        while (timer < _boostTime)
        {
            // 대쉬 런 애니메이션
            _currentAnimatorHash = DashRun_Hash;
            _dashAni.Play(DashRun_Hash);

            // 대쉬 사운드 재생
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Dash);

            // 앞으로 전진하고 점프 가능
            _playerController._playerRigid.velocity = new Vector2(direction.x * _boostPower, _playerController._playerRigid.velocity.y);

            timer += Time.deltaTime;
            yield return null;
        }

        // 대쉬 정지 애니메이션
        _currentAnimatorHash = DashIdle_Hash;
        _dashAni.Play(DashIdle_Hash);
        _isBoost = false;
    }
}
