using Kst;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum MoveDir{Left = -1, None = 0 , Right =1};
    [SerializeField] private MoveDir dir;
    private Map3_PlayerController _player;
    private PlayerShooter _shooter;

    public void Init(Map3_PlayerController player) => _player = player;
    public void Init(PlayerShooter shooter)
    {
        _shooter = shooter;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => _shooter.OnAttackBtn());
    }

    /// <summary>
    /// 누르고 있는 동안 플레이어의 방향 설정
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_player != null)
            _player.SetDir((int)dir);
    }

    /// <summary>
    /// 손에서 뗄 때, 플레이어의 방향 설정
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_player != null)
            _player.SetDir(0);
    }
}