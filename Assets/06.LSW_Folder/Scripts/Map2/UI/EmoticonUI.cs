using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class EmoticonUI : MonoBehaviourPun
{
    [Header("Emoticon Panel UI Reference")]
    [SerializeField] private Button _smileEmoticon;
    [SerializeField] private Button _quizEmoticon;
    [SerializeField] private Button _surpriseEmoticon;
    [SerializeField] private Button _angryEmoticon;
    [SerializeField] private Button _loveEmoticon;
    [SerializeField] private Button _weepEmoticon;

    private event Action _onSmileBtn;
    private event Action _onQuizBtn;
    private event Action _onSurpriseBtn;
    private event Action _onAngryBtn;
    private event Action _onLoveBtn;
    private event Action _onWeepBtn;
    
    private void Start()
    {
        _smileEmoticon.onClick.AddListener(() => _onSmileBtn?.Invoke());
        _quizEmoticon.onClick.AddListener(() => _onQuizBtn?.Invoke());
        _surpriseEmoticon.onClick.AddListener(() => _onSurpriseBtn?.Invoke());
        _angryEmoticon.onClick.AddListener(() => _onAngryBtn?.Invoke());
        _loveEmoticon.onClick.AddListener(() => _onLoveBtn?.Invoke());
        _weepEmoticon.onClick.AddListener(() => _onWeepBtn?.Invoke());    
    }

    public void Initialize(
        Action onSmileBtn,
        Action onQuizBtn,
        Action onSurpriseBtn,
        Action onAngryBtn,
        Action onLoveBtn,
        Action onWeepBtn)
    {
        _onSmileBtn = onSmileBtn;
        _onQuizBtn = onQuizBtn;
        _onSurpriseBtn = onSurpriseBtn;
        _onAngryBtn = onAngryBtn;
        _onLoveBtn = onLoveBtn;
        _onWeepBtn = onWeepBtn;
    }
}
