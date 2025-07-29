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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_player != null)
            _player.SetDir((int)dir);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_player != null)
            _player.SetDir(0);
    }
}