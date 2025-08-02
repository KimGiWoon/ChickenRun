using System;
using System.Collections;
using Kst;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Map3 : MonoBehaviourPun
{
    [Header("Play Time UI Reference")]
    [SerializeField] TMP_Text _playTimeText;

    [Header("Egg Count UI Reference")]
    [SerializeField] TMP_Text _eggCountText;

    [Header("Button UI Reference")]
    [SerializeField] Button _optionButton, _emoticonButton;

    [Header("Start Panel Reference")]
    [SerializeField] GameObject _startPanel;
    [SerializeField] TMP_Text _countText;
    [SerializeField] float _routineTime = 1f;
    [SerializeField] GameObject _wall; //TODO <김승태> 이 타이밍에 spwanPlate 호출하도록 변경하기.

    [Header("Goal Slider UI Reference")]
    [SerializeField] Transform _playerPosition;

    [Header("Option Panel UI Reference")]
    [SerializeField] GameObject _optionPanel;
    [SerializeField] Button _backButton, _exitButton;
    [SerializeField] Slider _musicSlider, _effectSlider;
    [SerializeField] Toggle _camModeCheckToggle;
    [SerializeField] CameraController_Map3 _cameraController; //TODO <김승태> 추후 수정부탁드립니다.

    [Header("Emoticon Panel UI Reference")]
    [SerializeField] PlayerEmoticonController_Map3 _playerEmoticonController;
    [SerializeField] GameObject _emoticonPanel;
    [SerializeField] Sprite[] _emoticonSprite;
    [SerializeField] Button _smileEmoticon, _quizEmoticon, _surpriseEmoticon, _angryEmoticon, _loveEmoticon, _weepEmoticon;

    Coroutine _panelRoutine;
    Coroutine _emoticonRoutine;
    public bool _isOptionOpen = false;
    public bool _isEmoticonPanelOpen = false;
    public float _emoticonTime = 3f;

    public event Action OnGameStart;

    // 맵타입 설정
    private void OnEnable()
    {
        GameManager_Map3.Instance._currentMapType = "Map3";
    }

    private void Start()
    {
        // // 달걀 획득 UI 이벤트 구독
        // GameManager_Map3.Instance.OnEggCountChange += UpdateGetEggUI;
        // GameManager_Map3.Instance.OnEggCountChange += UpdateGetEggUI;
        // 시작할 시 획득한 달걀은 0이므로 UI설정
        // UpdateGetEggUI(0);

        InGameUIInit();
        SoundVolumeInit();
    }

    private void Update()
    {
        // 플레이 타임 UI 출력
        if (GameManager_Map3.Instance == null) Debug.Log("게임 매니저 인스턴스 null");
        _playTimeText.text = GameManager_Map3.Instance.PlayTimeUpdate();
    }

    private void OnDestroy()
    {
        // // 달걀 획득 UI 이벤트 해제
        // GameManager_Map3.Instance.OnEggCountChange -= UpdateGetEggUI;
        // GameManager_Map3.Instance.OnEggCountChange -= UpdateGetEggUI;

        // 메모리 누수 방지로 리셋
        _camModeCheckToggle.onValueChanged.RemoveListener(CamModeCheck);
        _optionButton.onClick.RemoveListener(OnOptionWindow);
        _backButton.onClick.RemoveListener(OffOptionWindow);
        _exitButton.onClick.RemoveListener(OnExitPlayGame);
        _emoticonButton.onClick.RemoveListener(OnEmoticonPanel);
    }

    // 달걀 획득 UI
    // private void UpdateGetEggUI(int totalEgg)
    // {
    //     _eggCountText.text = $"x {totalEgg}";
    // }

    // 버튼, 토글 UI 초기화
    private void InGameUIInit()
    {
        // 카메라 모드 체크 토글 
        _camModeCheckToggle.onValueChanged.AddListener(CamModeCheck);

        // 사운드 볼륨
        _musicSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetBGM(volume));
        _effectSlider.onValueChanged.AddListener((volume) => SettingManager.Instance.SetSFX(volume));

        // UI 버튼
        _optionButton.onClick.AddListener(() => OnOptionWindow());
        _backButton.onClick.AddListener(() => OffOptionWindow());
        _exitButton.onClick.AddListener(() => OnExitPlayGame());
        _emoticonButton.onClick.AddListener(() => OnEmoticonPanel());
        _smileEmoticon.onClick.AddListener(() => OnEmoticon(0));
        _quizEmoticon.onClick.AddListener(() => OnEmoticon(1));
        _surpriseEmoticon.onClick.AddListener(() => OnEmoticon(2));
        _angryEmoticon.onClick.AddListener(() => OnEmoticon(3));
        _loveEmoticon.onClick.AddListener(() => OnEmoticon(4));
        _weepEmoticon.onClick.AddListener(() => OnEmoticon(5));
    }

    // 카메라 모드 체크 (토글)
    private void CamModeCheck(bool check)
    {
        _cameraController._isDelayMove = check;
    }

    // BGM, SFX 볼륨 초기화
    private void SoundVolumeInit()
    {
        _musicSlider.value = SettingManager.Instance.BGM.Value;
        _effectSlider.value = SettingManager.Instance.SFX.Value;

        // 사운드 초기값 중간 세팅
        _musicSlider.value = 0.1f;
        _effectSlider.value = 0.2f;
    }

    // 옵션 창 오픈
    private void OnOptionWindow()
    {
        _optionPanel.SetActive(true);
        _isOptionOpen = true;
    }

    // 옵션 창 닫기
    private void OffOptionWindow()
    {
        _optionPanel.SetActive(false);
        _isOptionOpen = false;
    }

    // 이모티콘 패널 오픈
    private void OnEmoticonPanel()
    {
        if (!_isEmoticonPanelOpen)
        {
            _emoticonPanel.SetActive(true);
            _isEmoticonPanelOpen = true;
        }
        else
        {
            _emoticonPanel.SetActive(false);
            _isEmoticonPanelOpen = false;
        }
    }

    // 플레이어 이모티콘 컨트롤러 가져오기
    public void GetPlayerEmoticonController(GameObject emoticonController)
    {
        _playerEmoticonController = emoticonController.GetComponent<PlayerEmoticonController_Map3>();
    }

    // 이모티콘 표시
    private void OnEmoticon(int index)
    {
        // 이모티콘 말풍성 활성화 및 이모티콘 표시
        _playerEmoticonController._SpeechBubble.SetActive(true);
        _playerEmoticonController._emoticonImage.sprite = _emoticonSprite[index];

        // 이모티콘을 사용하면 패널 비활성화
        _emoticonPanel.SetActive(false);
        _isEmoticonPanelOpen = false;

        // 실행중인 코루틴 무시하고 코루틴 실행
        if (_emoticonRoutine != null)
        {
            StopCoroutine(_emoticonRoutine);
        }
        // 이모티콘 표시 시간 코루틴 시작
        _emoticonRoutine = StartCoroutine(EmoticonPlayTimeCoroutine());

        PhotonView playerView = PhotonView.Get(_playerPosition.gameObject);
        Debug.Log($"플레이어 뷰 : {playerView}");
        if (playerView != null && playerView.IsMine)
        {
            // 자신을 제외한 플레이어에게 이모티콘 표시
            playerView.RPC(nameof(_playerEmoticonController.EmoticonPlay), RpcTarget.Others, index);
            Debug.Log("뿌려주기 실행");
        }
    }

    // 나가기 버튼 클릭
    private void OnExitPlayGame()
    {
        string exitPlayer = PhotonNetwork.LocalPlayer.NickName;

        // 나감을 알림
        photonView.RPC(nameof(ExitPlayer), RpcTarget.AllViaServer, exitPlayer);
    }

    // 방 나가기
    [PunRPC]
    private void ExitPlayer(string PlayerNickname)
    {
        Debug.Log($"{PlayerNickname}께서 나갔습니다.");

        // 현재의 방을 나가기
        //PhotonNetwork.LeaveRoom();

        // 로비 씬이 있으면 추가해서 씬 이동
        PhotonNetwork.LoadLevel("MainScene");
    }

    // 플레이어 위치 세팅
    public void SetPlayerPosition(Transform player)
    {
        _playerPosition = player;
    }

    // 스타트 코루틴
    [PunRPC]
    public void StartGameRoutine()
    {
        if (_panelRoutine == null)
        {
            _panelRoutine = StartCoroutine(StartPanelCoroutine());
        }
        else
        {
            _panelRoutine = null;
        }
    }

    // 스타트 패널 UI 코루틴
    private IEnumerator StartPanelCoroutine()
    {
        WaitForSeconds time = new WaitForSeconds(_routineTime);
        int count = 4;

        yield return new WaitForSeconds(2f);
        _startPanel.SetActive(true);

        while (true)
        {
            if (count == 1)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Start);
                _countText.text = "사격 개시";
            }
            else
            {
                SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Count);
                _countText.text = (count - 1).ToString();
            }

            yield return time;
            count--;

            if (count == 0)
            {
                SoundManager.Instance.PlayBGM(SoundManager.Bgms.BGM_InGame4);
                _startPanel.SetActive(false);
                //TODO <김승태> 스포너 생성 시작 시점
                OnGameStart?.Invoke();
                // GameManager_Map3.Instance.PlateSpawner.StartSpawn();
                break;
            }
        }
    }

    // 이모티콘 표시 코루틴
    private IEnumerator EmoticonPlayTimeCoroutine()
    {
        yield return new WaitForSeconds(_emoticonTime);

        _playerEmoticonController._SpeechBubble.SetActive(false);
    }


}
