using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerDataManager : GenericSingleton<PlayerDataManager>
{
    [LabelText("시작 목숨 값")]
    [SerializeField] private int startCount;
    
    [LabelText("현재 플레이어 목숨 값")]
    [SerializeField] private int tryCount;
    
    [LabelText("플레이어 인벤토리 데이터")]
    [SerializeField] private List<ItemData> playerInven;

    [LabelText("사진 봤는지 여부")]
    [SerializeField] private bool isLookedPic;

    private void Start()
    {
        // 시작시 목숨 값 지정으로 초기화
        tryCount = startCount;
    }
}
