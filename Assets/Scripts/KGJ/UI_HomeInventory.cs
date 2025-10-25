using System;
using System.Collections; // 코루틴 사용을 위해 추가
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HomeInventory : MonoBehaviour
{
    // ⬇️ 수정: slotParent가 곧 제출 애니메이션이 진행될 부모가 됩니다.
    [SerializeField] private Transform slotParent; 
    [SerializeField] private GameObject slotPrefab;

    // ⬇️ 제출 애니메이션의 최종 도착 지점 (화면 중앙)
    [SerializeField] private RectTransform submissionTarget;
    
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
        if (panelImageComponent != null && panelImages.Count > 0)
        {
            panelImageComponent.sprite = panelImages[0];
        }
        
        // ⬇️ submissionTarget이 할당되지 않았다면 화면 중앙을 기본값으로 설정
        if (submissionTarget == null)
        {
            GameObject centerGO = new GameObject("SubmissionTarget");
            centerGO.transform.SetParent(transform.root); // 최상위 캔버스 아래에 둠
            submissionTarget = centerGO.AddComponent<RectTransform>();
            submissionTarget.anchorMin = submissionTarget.anchorMax = new Vector2(0.5f, 0.5f);
            submissionTarget.pivot = new Vector2(0.5f, 0.5f);
            submissionTarget.sizeDelta = Vector2.zero;
        }

        InitializeSlots();
        OnInventoryChanged(); 
    }
    
    private void OnEnable()
    {
        HomeSlot.OnSlotClicked += OnSlotClicked;
        HomeSlot.OnDropItemRequested += OnDropItemRequested;
    }

    private void OnDisable()
    {
        HomeSlot.OnSlotClicked -= OnSlotClicked;
        HomeSlot.OnDropItemRequested -= OnDropItemRequested;
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
            
            HomeSlot slot = slotGO.GetComponent<HomeSlot>();
            
            if (slot != null)
            {
                slot.Init(i); 
                _slots.Add(slot);
            }
            else
            {
                Debug.LogError("Slot Prefab에 HomeSlot 컴포넌트가 없습니다!", slotGO);
            }
        }
    }
    
    public void OnInventoryChanged()
    {
        if (PlayerInventory.Instance == null) return;
        
        IReadOnlyList<ItemCsvRow> currentItems = PlayerInventory.Instance.Inventory;
        int maxSlots = PlayerInventory.Instance.MaxSlots;
        int currentItemCount = PlayerInventory.Instance.ItemCount; 

        for (int i = 0; i < maxSlots; i++)
        {
            ItemCsvRow item = (i < currentItems.Count) ? currentItems[i] : null;
            
            if (i < _slots.Count)
            {
                _slots[i].UpdateSlot(item);
            }
        }

        // ... (자동 선택 로직은 동일) ...
        if (currentItemCount > _previousItemCount) 
        {
            ItemCsvRow newItem = currentItems[currentItems.Count - 1]; 
            int newSlotIndex = currentItems.Count - 1;
            OnSlotClicked(newSlotIndex, newItem); 
        }
        else if (currentItemCount < _previousItemCount)
        {
            if (_selectedItem != null && !currentItems.Contains(_selectedItem))
            {
                if (currentItemCount > 0)
                {
                    OnSlotClicked(0, currentItems[0]);
                }
                else
                {
                    OnSlotClicked(-1, null);
                }
            }
        }

        _previousItemCount = currentItemCount; 
        SetInfoPanel(_selectedItem);
    }
    
    private void OnSlotClicked(int slotIndex, ItemCsvRow item)
    {
        if (item != null)
        {
            _selectedItem = item;
        } 
        
        _selectedSlotIndex = slotIndex;
        SetInfoPanel(item);
        
        int imageIndex = (item != null) ? _selectedSlotIndex + 1 : 0; 
        
        if (panelImageComponent != null && panelImages.Count > imageIndex)
        {
            panelImageComponent.sprite = panelImages[imageIndex];
        }
    }
    
    // HomeSlot.OnDropItemRequested 이벤트 핸들러 (아이템 제출 로직)
    private void OnDropItemRequested(ItemCsvRow item)
    {
        // 1. 제출 애니메이션 시작 (애니메이션 완료 후 파괴 로직 포함)
        HomeSlot sourceSlot = _slots.FirstOrDefault(s => s.GetItem() == item);
        if (sourceSlot != null)
        {
            StartCoroutine(StartSubmissionAnimation(item, sourceSlot.transform as RectTransform));
        }
        
        // 2. 제출 후, _selectedItem 초기화 (제출된 아이템은 이제 인벤토리에 없음)
        _selectedItem = null;
        SetInfoPanel(null); // 정보 패널 초기화
    }
    
    // ⬇️ 아이템 제출 애니메이션 코루틴
    private IEnumerator StartSubmissionAnimation(ItemCsvRow item, RectTransform startTransform)
    {
        if (item.itemSprite == null)
        {
            Debug.LogError("제출 아이템에 스프라이트가 없습니다!");
            yield break;
        }
        
        // 1. 애니메이션용 임시 UI 오브젝트 생성 (슬롯 프리팹 재사용)
        GameObject tempGO = Instantiate(slotPrefab, transform.root); // 캔버스 최상위 부모 아래에 생성
        RectTransform tempRect = tempGO.GetComponent<RectTransform>();
        Image tempImage = tempGO.GetComponentInChildren<Image>(); // 아이콘 이미지 컴포넌트

        // HomeSlot 스크립트가 있다면 제거 (애니메이션 중 상호작용 방지)
        HomeSlot tempHomeSlot = tempGO.GetComponent<HomeSlot>();
        if (tempHomeSlot != null) Destroy(tempHomeSlot);
        
        // 2. 초기 설정: 위치 설정 및 아이콘 업데이트
        tempRect.position = startTransform.position; // 시작 위치는 클릭된 슬롯의 위치
        tempImage.sprite = item.itemSprite; 
        tempImage.enabled = true;
        
        // 애니메이션 설정
        float duration = 2f; // 애니메이션 지속 시간
        float startTime = Time.time;
        Vector3 startPosition = tempRect.position;
        Vector3 targetPosition = submissionTarget.position; // 화면 중앙

        // 3. 애니메이션 실행
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            
            // 부드러운 시작/종료를 위해 Ease-in-out 사용
            t = t * t * (3f - 2f * t); 
            
            tempRect.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            // 크기/투명도 변화도 추가 가능 (선택 사항)
            // tempRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, t);
            
            yield return null;
        }

        // 4. 최종 위치 설정 및 파괴
        tempRect.position = targetPosition;
        
        // TODO: 여기에서 게임 로직에 제출 완료 신호를 보냅니다.
        Debug.Log($"[Submit] {item.itemName} 제출 애니메이션 완료. 파괴됨.");
        
        Destroy(tempGO);
    }
    
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