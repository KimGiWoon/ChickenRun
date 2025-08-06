using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel_LoginScene : UIBase
{
    [SerializeField] private Button _touchToSButton;
    [SerializeField] private TMP_Text _touchToSText;
    [SerializeField] private float _blinkSecond;

    private Coroutine _blinkCoroutine;

    private void OnEnable()
    {
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    private void OnDisable()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        _touchToSText.enabled = true;
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            _touchToSText.enabled = !_touchToSText.enabled;
            yield return new WaitForSeconds(_blinkSecond);
        }
    }

}
