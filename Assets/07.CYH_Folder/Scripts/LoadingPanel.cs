using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingPanel : UIBase
{
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private float _dotSecond;

    private Coroutine _typingCoroutine;

    private void OnEnable()
    {
        _typingCoroutine = StartCoroutine(TypingDots());
    }

    private void OnDisable()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);
    }

    IEnumerator TypingDots()
    {
        string baseText = "Loading";
        string dots = "···";
        int dotCount = 0;

        while (true)
        {
            // 0 ~ 3 순환
            dotCount = (dotCount + 1) % 4;
            _loadingText.text = baseText + dots.Substring(0, dotCount);
            yield return new WaitForSeconds(_dotSecond);
        }
    }
}
