using UnityEngine;
using TMPro;
using System.Collections;

public class UI_InteractHint : MonoBehaviour
{
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float fadeSpeed = 8f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        PlayerInteract.OnTargetChanged += HandleTargetChanged;
        PlayerInteract.OnHoldStart += HandleHoldStart;
        PlayerInteract.OnHoldEnd += HandleHoldEnd;
    }

    private void OnDisable()
    {
        PlayerInteract.OnTargetChanged -= HandleTargetChanged;
        PlayerInteract.OnHoldStart -= HandleHoldStart;
        PlayerInteract.OnHoldEnd -= HandleHoldEnd;
    }

    private void HandleTargetChanged(IInteractable newTarget)
    {
        if (newTarget == null)
        {
            FadeTo(0f);
            return;
        }

        transform.position = (newTarget as MonoBehaviour).transform.position + worldOffset;
        hintText.text = newTarget.GetInteractPrompt();
        FadeTo(1f);
    }

    private void HandleHoldStart(Transform _)
    {
        FadeTo(0f);
    }

    private void HandleHoldEnd()
    {
        FadeTo(1f);
    }

    private void FadeTo(float alpha)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(alpha));
    }

    private IEnumerator FadeRoutine(float target)
    {
        while (!Mathf.Approximately(_canvasGroup.alpha, target))
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, target, Time.deltaTime * fadeSpeed);
            yield return null;
        }
    }
}