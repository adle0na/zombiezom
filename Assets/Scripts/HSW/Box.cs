using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static IInteractable;
using DG.Tweening;

public class Box : MonoBehaviour, IInteractable
{
    [LabelText("박스 이미지")]
    public SpriteRenderer boxSprite;
    [LabelText("박스 데이터")]
    public BoxData boxData;
    public InteractHoldType HoldType { get; } = InteractHoldType.Long;
    public bool IsInteractable => !boxData.isOpened;
    private GameObject _dropItemPrefab;
    private GameObject _catBoxPrefab;
    
    // 아이템 드롭 시 필요한 설정 값
    private float dropForce = 2f; // 초기 밀어내는 힘
    private float dropDuration = 0.5f; // DOTween 낙하 애니메이션 시간
    private float maxHorizontalSpread = 1f; // 아이템이 퍼지는 최대 수평 거리

    private void Awake()
    {
        _dropItemPrefab = Resources.Load<GameObject>("Prefabs/DropItem");
        _catBoxPrefab = Resources.Load<GameObject>("Prefabs/CatBox");
        boxSprite = GetComponent<SpriteRenderer>();
    }
    
    public void Interact()
    {
        boxData.isOpened = true;
        
        // 암것도 없으면 플레이어 머리위에 팝업 띄우기
        if (boxData.boxItems.Count == 0)
        {
            if (boxData.boxType == BoxType.NormalBox_S)
            {
                SoundManager.Instance.PlaySFX(3);
                GameObject go = Instantiate(_catBoxPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
                Destroy(go, 1f);
                Destroy(gameObject);
            }
            else if (UI_Popup.OnShowPopupRequested != null)
            {
                UI_Popup.OnShowPopupRequested.Invoke("텅 비어있다..."); 
            }
        }
        else // 하나라도 있으면 아이템 생성해서 바닥에 뿌리기
        {
            DropItems();
            Destroy(gameObject);
        }
    }

private void DropItems()
{
    if (_dropItemPrefab == null || boxData.boxItems.Count == 0) return;

    // 아이템이 떨어질 시작 위치 (박스의 중앙)
    Vector3 dropOrigin = transform.position;
    
    // 아이템 개수와 총 퍼짐 폭 계산
    int itemCount = boxData.boxItems.Count;
    float totalSpreadWidth = maxHorizontalSpread * 2f; // 아이템이 퍼질 수 있는 전체 가로 폭 (예: -1f부터 +1f까지 총 2f)

    for (int i = 0; i < itemCount; i++)
    {
        var item = boxData.boxItems[i];
        
        GameObject dropGO = Instantiate(_dropItemPrefab, dropOrigin, Quaternion.identity);
        
        DropItem dropItemComponent = dropGO.GetComponent<DropItem>();
        if (dropItemComponent != null)
        {
            dropItemComponent.Init(item);
            
            SpriteRenderer sr = dropGO.GetComponent<SpriteRenderer>();
            if(sr != null && item.itemSprite != null)
            {
                sr.sprite = item.itemSprite;
            }
        }
        
        Rigidbody2D rb = dropGO.GetComponent<Rigidbody2D>();
        
        float normalizedIndex = (itemCount > 1) ? (float)i / (itemCount - 1) : 0.5f;
        
        float targetXPosition = (normalizedIndex * totalSpreadWidth) - maxHorizontalSpread;
        
        float baseHorizontalForce = targetXPosition * (dropForce / maxHorizontalSpread); 
        
        float randomJitter = Random.Range(0.8f, 1.2f); // 80% ~ 120% 힘 변동
        float finalHorizontalForce = baseHorizontalForce * randomJitter;

        float verticalForce = dropForce * Random.Range(0.9f, 1.1f);
        Vector2 initialForce = new Vector2(finalHorizontalForce, verticalForce);
        
        rb.AddForce(initialForce, ForceMode2D.Impulse);
        
        dropGO.transform.DORotate(new Vector3(0, 0, Random.Range(-360, 360)), dropDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCirc);
    }
    
    boxData.boxItems.Clear();
}

    public string GetInteractPrompt()
    {
        return "열기";
    }
}
