using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_Inventory : MonoBehaviour
{
    public event Action<bool> OnEnableInventory;
    
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    // ⬇️ 새로 추가: 배경 이미지를 변경할 Image 컴포넌트
    [SerializeField] private Image panelImageComponent;
    
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    
    [SerializeField] private List<Sprite> panelImages = new List<Sprite>();
    
    private List<Slot> _slots = new List<Slot>();
    private bool _isOpened = false;

    private ItemCsvRow _selectedItem; 
    // ⬇️ 새로 추가: 현재 선택된 슬롯의 인덱스 (초기값 -1: 아무것도 선택 안됨)
    private int _selectedSlotIndex = -1;
    private int _previousItemCount = 0; // 이전 아이템 개수를 저장할 필드 추가

    private Transform _player;
    private GameObject _dropItemPrefab;
    
    private void Awake()
    {
        _dropItemPrefab = Resources.Load<GameObject>("Prefabs/DropItem");
        
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryUpdated += OnInventoryChanged;
            _previousItemCount = PlayerInventory.Instance.ItemCount; 
        }
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
        closeButton.onClick.AddListener(ToggleInventory);
        
        // 배경 이미지를 '아무것도 선택되지 않은' 초기 상태로 설정
        // panelImages[0]은 아무것도 선택되지 않았을 때의 이미지여야 합니다.
        if (panelImageComponent != null && panelImages.Count > 0)
        {
            panelImageComponent.sprite = panelImages[0];
        }

        InitializeSlots();
    }
    
    private void OnEnable()
    {
        // ⬇️ 이벤트 서명이 (int, ItemCsvRow)로 변경됨
        Slot.OnSlotClicked += OnSlotClicked;
        Slot.OnDropItemRequested += OnDropItemRequested;
    }

    private void OnDisable()
    {
        // ⬇️ 이벤트 서명이 (int, ItemCsvRow)로 변경됨
        Slot.OnSlotClicked -= OnSlotClicked;
        Slot.OnDropItemRequested -= OnDropItemRequested;
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
            Slot slot = slotGO.GetComponent<Slot>();
            
            // ⬇️ 슬롯에 자신의 인덱스를 전달
            slot.Init(i); 
            _slots.Add(slot);
        }
    }
    
    public void OnInventoryChanged()
    {
        if (PlayerInventory.Instance == null) return;
        
        IReadOnlyList<ItemCsvRow> currentItems = PlayerInventory.Instance.Inventory;
        int maxSlots = PlayerInventory.Instance.MaxSlots;
        int currentItemCount = PlayerInventory.Instance.ItemCount; 

        // 1. UI 슬롯 업데이트
        for (int i = 0; i < maxSlots; i++)
        {
            ItemCsvRow item = (i < currentItems.Count) ? currentItems[i] : null;
            _slots[i].UpdateSlot(item);
        }

        // 2. ★★★ 자동 선택 로직 추가 (인덱스 전달하도록 수정) ★★★
        
        if (currentItemCount > _previousItemCount) 
        {
            ItemCsvRow newItem = currentItems[currentItems.Count - 1]; 
            int newSlotIndex = currentItems.Count - 1;
            
            // ⬇️ 인덱스와 아이템을 함께 호출
            OnSlotClicked(newSlotIndex, newItem); 
        }
        else if (currentItemCount < _previousItemCount)
        {
            if (_selectedItem != null && !currentItems.Contains(_selectedItem))
            {
                if (currentItemCount > 0)
                {
                    // ⬇️ 첫 번째 슬롯 (인덱스 0)과 아이템을 함께 호출
                    OnSlotClicked(0, currentItems[0]);
                }
                else
                {
                    // ⬇️ 아무것도 선택 안 함 (인덱스 -1)과 null을 함께 호출
                    OnSlotClicked(-1, null);
                }
            }
        }

        // 3. 현재 아이템 개수를 저장
        _previousItemCount = currentItemCount; 
        
        // 4. 정보 패널 업데이트
        if (_isOpened)
        {
             SetInfoPanel(_selectedItem);
        }
    }

    public void ToggleInventory()
    {
        if (PlayerInventory.Instance == null) return;
        
        _isOpened = !_isOpened;
        OnEnableInventory?.Invoke(_isOpened);
        inventoryPanel.SetActive(_isOpened);

        if (_isOpened)
        {
            OnInventoryChanged(); 
            
            // 2. ★★★ 인벤토리에 아이템이 1개 이상이고, 선택된 아이템이 없을 때만 첫 번째 칸 자동 선택 ★★★
            if (PlayerInventory.Instance.ItemCount > 0 && _selectedItem == null)
            {
                ItemCsvRow firstItem = PlayerInventory.Instance.Inventory[0];
                
                // ⬇️ 첫 번째 슬롯 (인덱스 0)과 아이템을 함께 호출
                OnSlotClicked(0, firstItem); 
            }
            else if (_selectedItem == null)
            {
                 // 인벤토리가 비어있으면 정보 패널 초기화 및 배경 초기화
                 // ⬇️ 인덱스 -1, 아이템 null을 전달하여 선택 해제 상태로 배경 변경
                 OnSlotClicked(-1, null);
                 SetInfoPanel(null);
            }
        }
    }
    
    // Slot.OnSlotClicked 이벤트 핸들러 (아이템 정보 업데이트 및 배경 변경)
    // ⬇️ 슬롯 인덱스를 받도록 수정
    private void OnSlotClicked(int slotIndex, ItemCsvRow item)
    {
        _selectedItem = item;
        
        // 1. 슬롯 인덱스 저장
        _selectedSlotIndex = slotIndex;
        
        // 2. 정보 패널 업데이트
        SetInfoPanel(item);
        
        int imageIndex = _selectedSlotIndex + 1; // -1 -> 0, 0 -> 1, 4 -> 5
        
        if (panelImageComponent != null && panelImages.Count > imageIndex)
        {
            panelImageComponent.sprite = panelImages[imageIndex];
        }
    }
    
    // Slot.OnDropItemRequested 이벤트 핸들러
    private void OnDropItemRequested(ItemCsvRow item)
    {
        Vector3 position = FindAnyObjectByType<PlayerMovement>().transform.position;
        DropItem dropItem = Instantiate(_dropItemPrefab, position, Quaternion.identity).GetComponent<DropItem>();
        dropItem.Init(item);
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
            _title.text = "재료를 모아 수아를 구하자.";
            _description.text = "";
        }
    }
}