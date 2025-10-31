using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HomeInventory : MonoBehaviour
{
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private Image panelImageComponent;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private List<Sprite> panelImages = new List<Sprite>();
    
    private List<HomeSlot> _slots = new List<HomeSlot>();

    private ItemCsvRow _selectedItem;
    private int _selectedSlotIndex = -1;
    private int _previousItemCount = 0;

    private Transform _player;
    
    private void Awake()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryUpdated += OnInventoryChanged;
            _previousItemCount = PlayerInventory.Instance.ItemCount;
        }
    }

    private void Start()
    {
        // 배경 이미지를 '아무것도 선택되지 않은' 초기 상태로 설정
        if (panelImageComponent != null && panelImages.Count > 0)
        {
            panelImageComponent.sprite = panelImages[0];
        }

        InitializeSlots();
        // Start에서 OnInventoryChanged를 호출하여 초기 상태를 반영
        OnInventoryChanged();
    }
    
    private void OnEnable()
    {
        OnInventoryChanged();
        HomeSlot.OnSlotSelected += OnSlotSelected;
    }

    private void OnDisable()
    {
        HomeSlot.OnSlotSelected -= OnSlotSelected;
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryUpdated -= OnInventoryChanged;
        }
    }

    private void InitializeSlots()
    {
        if (PlayerInventory.Instance == null) return;
        
        int maxSlots = PlayerInventory.Instance.MaxSlots;
        
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            
            // ⬇️ 수정: HomeSlot 컴포넌트를 가져옵니다.
            HomeSlot slot = slotGO.GetComponent<HomeSlot>();
            
            if (slot != null)
            {
                slot.Init(i);
                _slots.Add(slot); // List<HomeSlot>에 추가
            }
            else
            {
                Debug.LogError("Slot Prefab에 HomeSlot 컴포넌트가 없습니다!", slotGO);
            }
        }
    }
    
    private void OnInventoryChanged()
    {
        if (PlayerInventory.Instance == null) return;
        
        IReadOnlyList<ItemCsvRow> currentItems = PlayerInventory.Instance.Inventory;
        int maxSlots = PlayerInventory.Instance.MaxSlots;
        int currentItemCount = PlayerInventory.Instance.ItemCount;

        // 1. UI 슬롯 업데이트
        for (int i = 0; i < maxSlots; i++)
        {
            // _slots가 List<HomeSlot>이지만, UpdateSlot 메서드는 동일하게 사용 가능
            ItemCsvRow item = (i < currentItems.Count) ? currentItems[i] : null;
            
            // 🚨 안전을 위해 _slots[i]가 존재하는지 확인
            if (i < _slots.Count)
            {
                _slots[i].UpdateSlot(item);
            }
        }

        // 2. ★★★ 자동 선택 로직 추가 (인덱스 전달하도록 수정) ★★★
        if (currentItemCount > _previousItemCount)
        {
            ItemCsvRow newItem = currentItems[currentItems.Count - 1];
            int newSlotIndex = currentItems.Count - 1;
            // ⬇️ 인덱스와 아이템을 함께 호출하여 자동 선택 및 배경 갱신
            OnSlotSelected(newSlotIndex, newItem);
        }
        else if (currentItemCount < _previousItemCount)
        {
            // 제거된 아이템이 현재 선택된 아이템인 경우, 선택을 갱신합니다.
            if (_selectedItem != null && !currentItems.Contains(_selectedItem))
            {
                if (currentItemCount > 0)
                {
                    OnSlotSelected(0, currentItems[0]);
                }
                else
                {
                    // 인벤토리가 비면 선택 초기화
                    OnSlotSelected(-1, null);
                }
            }
        }

        // 3. 현재 아이템 개수를 저장
        _previousItemCount = currentItemCount;
        
        // 4. 정보 패널 업데이트 (선택된 아이템 정보 유지)
        SetInfoPanel(_selectedItem);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryUseSelectedItem();
        }
    }

    private void OnSlotSelected(int slotIndex, ItemCsvRow item)
    {
        if (item != null)
        {
            _selectedItem = item;
            _selectedSlotIndex = slotIndex;
        }
        else
        {
            _selectedItem = null;
            _selectedSlotIndex = -1;
        }

        UpdateInfoPanel(_selectedItem);
        UpdatePanelBackground(_selectedSlotIndex, _selectedItem != null);
    }

    private void TryUseSelectedItem()
    {
        if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _slots.Count) return;

        HomeSlot slot = _slots[_selectedSlotIndex];
        if (slot == null) return;

        if (slot.TryUseItem())
        {
            UpdateInfoPanel(null);
            UpdatePanelBackground(-1, false);
        }
    }

    private void UpdateInfoPanel(ItemCsvRow item)
    {
        SetInfoPanel(item);
    }

    private void UpdatePanelBackground(int slotIndex, bool hasItem)
    {
        int imageIndex = hasItem && slotIndex >= 0 ? slotIndex + 1 : 0;
        
        if (panelImageComponent != null && panelImages.Count > imageIndex)
        {
            panelImageComponent.sprite = panelImages[imageIndex];
        }
    }
    
    // 정보 패널 업데이트 메서드
    private void SetInfoPanel(ItemCsvRow item)
    {
        if (item != null)
        {
            _title.text = item.itemName;
            _description.text = item.itemDes;
        }
        else
        {
            _title.text = "";
            _description.text = "";
        }
    }
}