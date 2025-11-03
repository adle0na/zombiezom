using System;
using System.Collections;
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
    public bool IsZombieInHome;

    [LabelText("피격 여부")]
    public bool canHit;

    [LabelText("애용이 찾았는지 여부")]
    public bool isFindCat;
    
    public List<ItemCsvRow> PlayerInventoryData => playerInven;
    public int PlayerFloor => playerFloor;
    public event Action OnHpDecreaseEvent;

    public event Action OnGetFullHpEvent;

    private Coroutine blinkCor;
    
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
        
        playerInven.Clear();
        
        playerInven.Add(ItemDataManager.Instance.GetItemByIndex(27));

        canHit = true;

        IsZombieInHome = false;
        isFindCat = false;
    }

    public void PlayerInHome()
    {
        OnGetFullHpEvent?.Invoke();
        
        // 체력 전부 회복
        tryCount = startCount;
    }
    
    public void GetHit()
    {
        if (!canHit) return;

        playerObj?.GetComponent<PlayerInteract>()?.ForceCancelInteraction();
        
        OnHpDecreaseEvent?.Invoke();

        SoundManager.Instance.PlaySFX(2);
        
        if (tryCount > 1)
        {
            tryCount--;
            
            UIManager.Instance.OpenBiteUI();

            if (blinkCor != null)
            {
                StopCoroutine(blinkCor);
            }

            blinkCor = StartCoroutine(BlinkCor());
        }
        else
            GameOver();
    }

    private void GameOver()
    {
        playerObj.SetActive(false);
        UIManager.Instance.OpenDeadUI();
        Debug.Log("게임 오버 처리");
    }

    IEnumerator BlinkCor()
    {
        SpriteRenderer playerSprite = playerObj.GetComponent<SpriteRenderer>();
        
        canHit = false;
        
        float duration = 2f;      // 깜빡임 총 시간 (1초)
        float blinkSpeed = 10f;   // 깜빡이는 속도(값이 높을수록 빠름)
        float time = 0f;

        Color color = playerSprite.color;

        while (time < duration)
        {
            // 0~1~0 식으로 반복되는 값 생성 (PingPong)
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            color.a = alpha;
            playerSprite.color = color;

            time += Time.deltaTime;
            yield return null;
        }

        // 마지막엔 알파값 복원
        color.a = 1f;
        playerSprite.color = color;
        
        canHit = true;
    }
}


