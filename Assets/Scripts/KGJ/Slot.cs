using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    // ⬇️ 이벤트 서명 변경: ItemCsvRow 대신 int slotIndex와 ItemCsvRow를 전달
    public static event Action<int, ItemCsvRow> OnSlotClicked;
    public static event Action<ItemCsvRow> OnDropItemRequested; 

    [SerializeField] private Image _itemIcon;
    
    private ItemCsvRow _currentItem;
    // ⬇️ 슬롯의 고유 인덱스 필드 추가
    private int _slotIndex = -1;
    
    // ⬇️ Init 메서드 수정: 인덱스를 받도록 수정
    public void Init(int index)
    {
        _slotIndex = index; // 인덱스 저장
        UpdateSlot(null); 
    }

    public void UpdateSlot(ItemCsvRow item)
    {
        _currentItem = item;

        if (_currentItem != null)
        {
            _itemIcon.sprite = _currentItem.itemSprite; 
            _itemIcon.enabled = true;
        }
        else
        {
            _itemIcon.sprite = null;
            _itemIcon.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        // 좌클릭: 상단 정보 패널 업데이트 및 배경 변경 요청
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // ⬇️ 자신의 인덱스와 아이템을 함께 전달
            OnSlotClicked?.Invoke(_slotIndex, _currentItem);
        }
        
        // 우클릭: 아이템 버리기
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnDropItem();
        }
    }
    
    private void OnDropItem()
    {
        if (_currentItem == null) return;
        
        ItemCsvRow itemToDrop = _currentItem;
        
        // 1. 인벤토리 데이터에서 제거 요청
        if (PlayerInventory.Instance.RemoveItemByIndex(itemToDrop.index)) 
        {
            // 2. 아이템 버리기 이벤트 발생
            OnDropItemRequested?.Invoke(itemToDrop); 
            
            // 3. 정보 패널 초기화 요청 및 배경을 '아무것도 선택되지 않은' 상태로 변경 요청
            // ⬇️ 인덱스 -1과 null을 전달하여 선택 해제 상태로 변경 요청
            OnSlotClicked?.Invoke(-1, null); 
            
            // **UI 갱신은 PlayerInventory의 이벤트 구독 로직이 처리합니다.**
        }
    }
}