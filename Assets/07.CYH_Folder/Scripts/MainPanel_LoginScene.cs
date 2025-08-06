using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel_LoginScene : UIBase
{
    [SerializeField] private Button _startButton;
    [SerializeField] private TMP_Text _startText;
    [SerializeField] private float _blinkSecond;

    public Action OnClickTouch { get; set; }

    private Coroutine _blinkCoroutine;

    private void Start()
    {
        _startButton.onClick.AddListener(() => OnClickTouch.Invoke());
    }

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
        _startText.enabled = true;
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            _startText.enabled = !_startText.enabled;
            yield return new WaitForSeconds(_blinkSecond);
        }
    }

}
