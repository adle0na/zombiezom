using UnityEngine;
using UnityEngine.UI;

public class UI_HoldProgress : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private CanvasGroup _canvasGroup;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float fadeSpeed = 8f;

    private Transform _target;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        // 🎧 이벤트 구독
        PlayerInteract.OnHoldStart += HandleHoldStart;
        PlayerInteract.OnHoldProgress += HandleHoldProgress;
        PlayerInteract.OnHoldEnd += HandleHoldEnd;
    }

    private void OnDisable()
    {
        // 🎧 구독 해제
        PlayerInteract.OnHoldStart -= HandleHoldStart;
        PlayerInteract.OnHoldProgress -= HandleHoldProgress;
        PlayerInteract.OnHoldEnd -= HandleHoldEnd;
    }

    private void HandleHoldStart(Transform target)
    {
        _target = target;
        transform.position = _target.position + worldOffset;
        fillImage.fillAmount = 0f;
        FadeTo(1f);
    }

    private void HandleHoldProgress(float ratio)
    {
        fillImage.fillAmount = ratio;
    }

    private void HandleHoldEnd()
    {
        _target = null;
        FadeTo(0f);
        fillImage.fillAmount = 0f;
    }

    private void FadeTo(float alpha)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(alpha));
    }

    private System.Collections.IEnumerator FadeRoutine(float target)
    {
        while (!Mathf.Approximately(_canvasGroup.alpha, target))
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.deltaTime * fadeSpeed);
            yield return null;
        }
    }
}
