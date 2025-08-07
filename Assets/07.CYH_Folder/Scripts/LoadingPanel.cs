using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingPanel : UIBase
{
    [SerializeField] private TMP_Text _loadingText;
    [SerializeField] private float _dotSecond;
    [SerializeField] private float _soundSecond;

    private Coroutine _typingCoroutine;
    private Coroutine _sfxCoroutine;

    private void OnEnable()
    {
        _typingCoroutine = StartCoroutine(TypingCoroutine());
        _sfxCoroutine = StartCoroutine(SFXCoroutine());
    }

    private void OnDisable()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        if (_sfxCoroutine != null)
        {
            StopCoroutine(_sfxCoroutine);
        }
    }

    IEnumerator TypingCoroutine()
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

    IEnumerator SFXCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_soundSecond);
            SoundManager.Instance.PlaySFX(SoundManager.Sfxs.SFX_Loading);
        }
    }
}
