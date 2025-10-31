using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HomeSlot : MonoBehaviour, IPointerClickHandler
{
    public static event Action<int, ItemCsvRow> OnSlotSelected;
    public static event Action<ItemCsvRow> OnDropItemRequested;

    [SerializeField] private Image _itemIcon;
    
    private ItemCsvRow _currentItem;
    private int _slotIndex = -1;
    
    private HomeUIScript _homeUI;
        
    private void Start()
    {
        _homeUI = GetComponentInParent<HomeUIScript>();
    }
    
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

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnSlotSelected?.Invoke(_slotIndex, _currentItem);
        }
    }

    public bool TryUseItem()
    {
        if (_currentItem == null) return false;

        if (_homeUI.remainCure.Count <= 0) return false;
        if (_homeUI.remainCure[0] != _currentItem.index) return false;

        ItemCsvRow itemToDrop = _currentItem;

        // 1. 인벤토리 데이터에서 제거 요청 (슬롯 인덱스 사용이 권장됨)
        // 현재 코드는 ItemCsvRow의 데이터 ID를 사용합니다. (기존 로직 유지)
        if (PlayerInventory.Instance.RemoveItemByIndex(itemToDrop.index))
        {
            // 2. 아이템 제출 이벤트 발생
            OnDropItemRequested?.Invoke(itemToDrop);

            // 3. 선택 초기화 요청
            OnSlotSelected?.Invoke(-1, null);

            _homeUI.RemoveFirst();
            return true;
        }

        return false;
    }
    
    public ItemCsvRow GetItem()
    {
        return _currentItem;
    }
}