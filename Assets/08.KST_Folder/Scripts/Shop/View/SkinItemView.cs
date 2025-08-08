using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinItemView : UIBase
{
    [SerializeField] private TMP_Text _nameText, _priceText;
    [SerializeField] private Image _skinImage;
    public Button _purchaseBtn;


    private Sprite _sprite;
    private string _name;
    private int _price;
    private bool _isPurchsed;

    /// <summary>
    /// 데이터 세팅
    /// </summary>
    /// <param name="image"></param>
    /// <param name="name"></param>
    /// <param name="price"></param>
    /// <param name="IsPurchased"></param>
    public void SetData(Sprite image, string name, int price, bool IsPurchased)
    {
        _sprite = image;
        _name = name;
        _price = price;
        _isPurchsed = IsPurchased;
    }
    /// <summary>
    /// 구매 시 버튼 활성화 여부
    /// </summary>
    /// <param name="isPurchased">구매 여부</param>
    public void RefreshPurchase(bool isPurchased)
    {
        _isPurchsed = isPurchased;
        _purchaseBtn.interactable = !isPurchased;
    }

    /// <summary>
    /// UI 갱신
    /// </summary>
    public override void RefreshUI()
    {
        _skinImage.sprite = _sprite;
        _nameText.text = _name;
        if (!_isPurchsed)
            _priceText.text = $"{_price} 골드";
        else
        {
            _priceText.text = "이미 구매함.";
            _purchaseBtn.interactable = !_isPurchsed;
        }
    }
}

