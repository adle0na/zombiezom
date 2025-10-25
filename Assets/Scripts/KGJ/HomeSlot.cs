using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ⬇️ IPointerExitHandler 추가
public class HomeSlot : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public static event Action<int, ItemCsvRow> OnSlotClicked;
    public static event Action<ItemCsvRow> OnDropItemRequested; 

    [SerializeField] private Image _itemIcon;
    
    private ItemCsvRow _currentItem;
    private int _slotIndex = -1;
    
    public void Init(int index)
    {
        _slotIndex = index;
        UpdateSlot(null); 
    }

    public void UpdateSlot(ItemCsvRow item)
    {
        _currentItem = item;

        if (_currentItem != null)
        {
            // ... (아이콘 설정 로직) ...
            _itemIcon.sprite = _currentItem.itemSprite; 
            _itemIcon.enabled = true;
        }
        else
        {
            // ... (아이콘 초기화 로직) ...
            _itemIcon.sprite = null;
            _itemIcon.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        // 좌클릭: 아이템 제출
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnDropItem();
        }
    }
    
    private void OnDropItem()
    {
        if (_currentItem == null) return;
        
        ItemCsvRow itemToDrop = _currentItem;
        
        // 1. 인벤토리 데이터에서 제거 요청 (슬롯 인덱스 사용이 권장됨)
        // 현재 코드는 ItemCsvRow의 데이터 ID를 사용합니다. (기존 로직 유지)
        if (PlayerInventory.Instance.RemoveItemByIndex(itemToDrop.index)) 
        {
            // 2. 아이템 제출 이벤트 발생
            OnDropItemRequested?.Invoke(itemToDrop); 
            
            // 3. 정보 패널 초기화 요청 및 배경을 '아무것도 선택되지 않은' 상태로 변경 요청
            OnSlotClicked?.Invoke(-1, null); 
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 호버 시작: 정보 패널 업데이트 및 배경 변경
        OnSlotClicked?.Invoke(_slotIndex, _currentItem);
    }
    
    // ⬇️ 새로 추가된 메서드: 마우스를 뗄 때 정보 초기화
    public void OnPointerExit(PointerEventData eventData)
    {
        // 호버 종료: 인덱스 -1과 null을 전달하여 정보 패널 초기화 및 배경 기본 상태로 되돌림
        OnSlotClicked?.Invoke(-1, null); 
    }

    public ItemCsvRow GetItem()
    {
        return _currentItem;
    }
}