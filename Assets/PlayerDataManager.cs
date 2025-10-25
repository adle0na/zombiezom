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
    [SerializeField] private List<ItemCsvRow> playerInven;

    [LabelText("현재 플레이어 층 위치")]
    public int playerFloor;

    [LabelText("플레이어 오브젝트")] 
    [SerializeField] public GameObject playerObj;
    
    [LabelText("사진 봤는지 여부")]
    [SerializeField] private bool isLookedPic;

    [LabelText("집에 좀비가 있는지 여부")]
    [SerializeField] private bool isZombieInHome;
    
    public List<ItemCsvRow> PlayerInventoryData => playerInven;
    public event Action OnHpDecreaseEvent;

    void Start()
    {
        ResetData();
    }
    
    public void ResetData()
    {
        // 시작시 목숨 값 지정으로 초기화
        tryCount = startCount;

        // 시작시 1층으로 표기
        playerFloor = 1;
    }

    public void GetHit()
    {
        OnHpDecreaseEvent?.Invoke();
        
        if (tryCount > 1)
        {
            tryCount--;
            GoBackFloor();
        }
        else
            GameOver();
    }

    private void GoBackFloor()
    {
        if (playerFloor > 1)
        {
            playerFloor--;
        }

        UIManager.Instance.OpenBiteUI();
        
        Debug.Log("아래로 내림");
        //캐릭터 무적, 반짝임 효과
    }
    
    private void GameOver()
    {
        UIManager.Instance.OpenDeadUI();
        Debug.Log("게임 오버 처리");
    }
}


