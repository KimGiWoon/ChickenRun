using Kst;
using UnityEngine;

public class Map3BtnUI : MonoBehaviour
{
    [SerializeField] private BtnHandler leftBtn;
    [SerializeField] private BtnHandler rightBtn;
    [SerializeField] private BtnHandler shootBtn;

    public void Init(Map3_PlayerController player)
    {
        leftBtn.Init(player);
        rightBtn.Init(player);
        shootBtn.Init(player.GetComponent<PlayerShooter>());
    }
}