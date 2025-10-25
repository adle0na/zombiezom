using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<ItemCsvRow, GameObject> OnItemPickupRequested;
    private ItemCsvRow _item;

    public void Init(ItemCsvRow item)
    {
        _item = item;
    }

    public IInteractable.InteractHoldType HoldType { get; }
    public bool IsInteractable { get; } = true;
    public void Interact()
    {
        if (gameObject != null)
            OnItemPickupRequested?.Invoke(_item, gameObject);
    }

    public string GetInteractPrompt()
    {
        return "줍기";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_ItemTooltip.OnShowTooltipRequested?.Invoke(_item, gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_ItemTooltip.OnHideTooltipRequested?.Invoke();
    }

    private void OnDestroy()
    {
        UI_ItemTooltip.OnHideTooltipRequested?.Invoke();
    }
}
