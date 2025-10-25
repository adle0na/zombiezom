using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : GenericSingleton<PlayerInventory>
{
    private int maxSlots = 5; // 인벤토리 최대 슬롯 개수

    // PlayerDataManager에서 데이터를 가져옵니다.
    private List<ItemCsvRow> CurrentInventory => PlayerDataManager.Instance.PlayerInventoryData;

    // 읽기 전용으로 노출
    public IReadOnlyList<ItemCsvRow> Inventory => CurrentInventory; 

    // ItemCount 속성 수정
    public int ItemCount => CurrentInventory.Count;
    
    /// <summary>
    /// 인벤토리가 가득 찼는지 여부
    /// </summary>
    public bool IsFull => CurrentInventory.Count >= maxSlots;

    // --- 인벤토리 기능 메서드 ---

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(ItemCsvRow item) 
    {
        if (IsFull)
        {
            Debug.LogWarning("[Inventory] 인벤토리가 가득 찼습니다!");
            return false;
        }

        // PlayerDataManager의 데이터에 직접 추가
        CurrentInventory.Add(item); 
        Debug.Log($"[Inventory] {item.itemName} (index:{item.index}) 추가됨. ({CurrentInventory.Count}/{maxSlots})");
        return true;
    }

    /// <summary>
    /// 아이템 삭제 (인덱스 기준 - ItemData의 itemIndex 필드 기준)
    /// </summary>
    public bool RemoveItemByIndex(int itemIndex)
    {
        // PlayerDataManager의 데이터에서 찾습니다.
        var target = CurrentInventory.Find(i => i.index == itemIndex);
        if (target != null)
        {
            // PlayerDataManager의 데이터에서 직접 제거
            CurrentInventory.Remove(target);
            Debug.Log($"[Inventory] {target.itemName} (index:{itemIndex}) 제거됨.");
            return true;
        }

        Debug.LogWarning($"[Inventory] index:{itemIndex} 아이템이 없습니다.");
        return false;
    }

    /// <summary>
    /// 전체 아이템 순회 (디버그용)
    /// </summary>
    public void PrintAllItems()
    {
        if (CurrentInventory.Count == 0)
        {
            Debug.Log("[Inventory] 비어 있음");
            return;
        }

        Debug.Log("===== [Inventory 목록] =====");
        foreach (var item in CurrentInventory)
        {
            Debug.Log($"index:{item.index} | {item.itemName} - {item.itemDes}");
        }
    }

    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    public void ClearInventory()
    {
        // PlayerDataManager의 데이터를 직접 초기화
        CurrentInventory.Clear();
        Debug.Log("[Inventory] 모든 아이템 삭제됨.");
    }
}