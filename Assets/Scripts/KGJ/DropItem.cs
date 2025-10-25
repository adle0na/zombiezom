using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<ItemData, GameObject> OnItemPickupRequested;
    private ItemData _item;

    public void Init(ItemData item)
    {
        _item = item;
    }

    private void Start()
    {
        ItemData testSword = new ItemData
        {
            // 2. 필드에 테스트 값을 할당합니다.
            itemIndex = 101,
            itemName = "낡은 검",
            itemSprite = null, // 요청하신 대로 null로 설정
            itemDes = "오래되어 녹슨 검입니다. 사용하기 까다롭습니다."
        };
        
        _item = testSword;
    }

    public IInteractable.InteractHoldType HoldType { get; }
    public bool IsInteractable { get; } = true;
    public void Interact()
    {
        // 플레이어 인벤토리 속으로 ㄱㄱ
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
