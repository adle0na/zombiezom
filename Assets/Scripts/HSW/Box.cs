using System.Collections.Generic;
using UnityEngine;
using static IInteractable;

public class Box : MonoBehaviour, IInteractable
{
    public SpriteRenderer boxSprite;
    public BoxData boxData;
    public InteractHoldType HoldType { get; } = InteractHoldType.Long;
    public bool IsInteractable => !boxData.isOpened;

    private void Awake()
    {
        boxSprite = GetComponent<SpriteRenderer>();
        boxData = new()
        {
            boxItems = new List<ItemData>()
            {

            },
            boxType = BoxType.BloodBox_L,
            isOpened = false,
        };
    }
    
    public void Interact()
    {
        boxData.isOpened = true;
        
        // 암것도 없으면 플레이어 머리위에 팝업 띄우기
        if (boxData.boxItems.Count == 0)
        {
            if (UI_Popup.OnShowPopupRequested != null)
            {
                UI_Popup.OnShowPopupRequested.Invoke("텅 비어있다..."); 
            }
        }

        // 하나라도 있으면 아이템 생성해서 바닥에 뿌리기
        // 열린 박스 스프라이트로 교체
    }

    public string GetInteractPrompt()
    {
        return "open";
    }
}
