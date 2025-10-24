using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI_Popup : MonoBehaviour
{
    public static Action<string> OnShowPopupRequested;
    private TMP_Text _text;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }
    
    private void OnEnable()
    {
        OnShowPopupRequested += ShowPopup;
    }

    private void OnDisable()
    {
        OnShowPopupRequested -= ShowPopup;
    }
    
    private void ShowPopup(string text)
    {
        _canvasGroup.DOKill(true);
        _text.text = text;
        FadeInAndOut();
    }
    
    private void FadeInAndOut()
    {
        const float fadeInDuration = 0.5f; // Fade In 시간
        const float stayDuration = 3.0f;    // 팝업 유지 시간
        const float fadeOutDuration = 0.5f; // Fade Out 시간

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_canvasGroup.DOFade(1f, fadeInDuration));
        sequence.AppendInterval(stayDuration);
        sequence.Append(_canvasGroup.DOFade(0f, fadeOutDuration));
    }

    private void OnDestroy()
    {
        _canvasGroup.DOKill(true);
    }
}