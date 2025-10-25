using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class UI_ItemTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Vector3 offset;
    private CanvasGroup _canvasGroup;

    public static Action<ItemCsvRow, GameObject> OnShowTooltipRequested;
    public static Action OnHideTooltipRequested;

    private const float FadeDuration = 0.2f;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    private void OnEnable()
    {
        OnShowTooltipRequested += ShowTooltip;
        OnHideTooltipRequested += HideTooltip;
    }

    private void OnDisable()
    {
        OnShowTooltipRequested -= ShowTooltip;
        OnHideTooltipRequested -= HideTooltip;
    }

    private void ShowTooltip(ItemCsvRow itemData, GameObject go)
    {
        transform.position = go.transform.position + offset;
        _title.text = itemData.itemName;
        _description.text = itemData.itemDes;
        
        _canvasGroup.DOKill(true);
        _canvasGroup.DOFade(1f, FadeDuration);
    }

    private void HideTooltip()
    {
        _canvasGroup.DOKill(true);
        _canvasGroup.DOFade(0f, FadeDuration);
    }

    private void OnDestroy()
    {
        _canvasGroup.DOKill(true);
    }
}