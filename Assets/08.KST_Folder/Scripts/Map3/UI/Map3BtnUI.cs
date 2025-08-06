using Kst;
using UnityEngine;
using UnityEngine.UI;

public class Map3BtnUI : MonoBehaviour
{
    [SerializeField] private BtnHandler leftBtn;
    [SerializeField] private BtnHandler rightBtn;
    [SerializeField] private BtnHandler shootBtn;

    [SerializeField] Image _cooldownImg;

    public void Init(Map3_PlayerController player)
    {
        var shooter = player.GetComponent<PlayerShooter>();

        leftBtn.Init(player);
        rightBtn.Init(player);
        shootBtn.Init(shooter);
        shooter.SetImg(_cooldownImg);
    }
}