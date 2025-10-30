using UnityEngine;
using TMPro;
using System.Collections;

public class UI_InteractHint : MonoBehaviour
{
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private float fadeSpeed = 8f;

    private CanvasGroup _canvasGroup;
    private IInteractable _currentTarget;
    private Transform _currentTargetTransform;
    private Transform _currentAnchorTransform;
    private bool _isHolding;
    private bool _isVisible;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _isVisible = false;
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

    private void Update()
    {
        if (_currentTarget == null)
            return;

        if (_currentTargetTransform == null || !_currentTargetTransform.gameObject.activeInHierarchy)
        {
            ClearTarget();
            return;
        }

        if (_currentAnchorTransform == null || !_currentAnchorTransform.gameObject.activeInHierarchy)
        {
            ClearTarget();
            return;
        }

        UpdateHintPosition();

        bool shouldBeVisible = !_isHolding && _currentTarget.IsInteractable;
        SetVisibility(shouldBeVisible);
    }

    private void HandleTargetChanged(IInteractable newTarget)
    {
        _currentTarget = newTarget;
        _currentTargetTransform = (newTarget as MonoBehaviour)?.transform;
        _currentAnchorTransform = newTarget?.HintAnchorTransform ?? _currentTargetTransform;

        if (_currentTarget == null || _currentTargetTransform == null || _currentAnchorTransform == null)
        {
            ClearTarget();
            return;
        }

        hintText.text = newTarget.GetInteractPrompt();
        UpdateHintPosition();

        bool shouldBeVisible = !_isHolding && newTarget.IsInteractable;
        SetVisibility(shouldBeVisible);
    }

    private void HandleHoldStart(Transform _)
    {
        _isHolding = true;
        SetVisibility(false);
    }

    private void HandleHoldEnd()
    {
        _isHolding = false;
        bool shouldBeVisible = _currentTarget != null && _currentTarget.IsInteractable;
        SetVisibility(shouldBeVisible);
    }

    private void ClearTarget()
    {
        _currentTarget = null;
        _currentTargetTransform = null;
        _currentAnchorTransform = null;
        _isHolding = false;
        hintText.text = string.Empty;
        SetVisibility(false);
    }

    private void UpdateHintPosition()
    {
        if (_currentTarget == null)
            return;

        Transform anchor = _currentTarget.HintAnchorTransform ?? _currentTargetTransform;

        if (anchor == null)
            return;

        _currentAnchorTransform = anchor;

        transform.position = anchor.position + _currentTarget.HintWorldOffset;
    }

    private void SetVisibility(bool shouldBeVisible)
    {
        if (_isVisible == shouldBeVisible)
            return;

        _isVisible = shouldBeVisible;
        FadeTo(shouldBeVisible ? 1f : 0f);
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