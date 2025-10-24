using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 10; // 인벤토리 최대 슬롯 개수

    [Header("Current Inventory")]
    public List<ItemData> inventory = new();

    /// <summary>
    /// 인벤토리가 가득 찼는지 여부
    /// </summary>
    public bool IsFull => inventory.Count >= maxSlots;

    /// <summary>
    /// 아이템 추가
    /// </summary>
    /// <param name="item">추가할 아이템 데이터</param>
    /// <returns>추가 성공 여부</returns>
    public bool AddItem(ItemData item)
    {
        if (IsFull)
        {
            Debug.LogWarning("[Inventory] 인벤토리가 가득 찼습니다!");
            return false;
        }

        inventory.Add(item);
        Debug.Log($"[Inventory] {item.itemName} (index:{item.itemIndex}) 추가됨. ({inventory.Count}/{maxSlots})");
        return true;
    }

    /// <summary>
    /// 아이템 삭제 (이름 기준)
    /// </summary>
    public bool RemoveItemByName(string itemName)
    {
        var target = inventory.Find(i => i.itemName == itemName);
        if (target != null)
        {
            inventory.Remove(target);
            Debug.Log($"[Inventory] {itemName} 제거됨.");
            return true;
        }

        Debug.LogWarning($"[Inventory] {itemName}은(는) 인벤토리에 없습니다.");
        return false;
    }

    /// <summary>
    /// 아이템 삭제 (인덱스 기준)
    /// </summary>
    public bool RemoveItemByIndex(int itemIndex)
    {
        var target = inventory.Find(i => i.itemIndex == itemIndex);
        if (target != null)
        {
            inventory.Remove(target);
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
        if (inventory.Count == 0)
        {
            Debug.Log("[Inventory] 비어 있음");
            return;
        }

        Debug.Log("===== [Inventory 목록] =====");
        foreach (var item in inventory)
        {
            Debug.Log($"index:{item.itemIndex} | {item.itemName} - {item.itemDes}");
        }
    }

    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    public void ClearInventory()
    {
        inventory.Clear();
        Debug.Log("[Inventory] 모든 아이템 삭제됨.");
    }
}
