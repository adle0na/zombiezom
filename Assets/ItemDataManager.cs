using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemDataManager : GenericSingleton<ItemDataManager>
{
    [Title("CSV 아이템 데이터 테이블")]
    [ReadOnly, LabelText("로드된 아이템 리스트")]
    public List<ItemCsvRow> itemList = new List<ItemCsvRow>();

    // 인덱스로 빠른 접근용 캐시 (선택)
    private Dictionary<int, ItemCsvRow> itemDict = new Dictionary<int, ItemCsvRow>();

    [Button("CSV 데이터 로드"), GUIColor(0.2f, 0.7f, 1f)]
    public void LoadCsvData()
    {
        // ✅ CSV에서 파싱해서 리스트 채우기
        itemList = CsvItemLoader.LoadDefault(); // Resources/Tb_ItemTable.csv

        // ✅ 딕셔너리 구성
        BuildDictionary();

        Debug.Log($"[ItemDataManager] 아이템 테이블 로드 완료 ({itemList?.Count ?? 0}개)");
    }

    private void BuildDictionary()
    {
        itemDict.Clear();
        foreach (var row in itemList)
        {
            if (itemDict.ContainsKey(row.index)) continue;
            itemDict[row.index] = row;
        }
    }

    /// <summary>
    /// Index 기반으로 아이템 데이터 조회
    /// </summary>
    public ItemCsvRow GetItemByIndex(int index)
    {
        if (itemDict.TryGetValue(index, out var row))
            return row;
        Debug.LogWarning($"[ItemTableManager] 인덱스 {index} 아이템이 존재하지 않습니다.");
        return null;
    }

    private void Start()
    {
        // 씬 시작 시 자동 로드 (원하면 주석처리 가능)
        LoadCsvData();
    }
}
