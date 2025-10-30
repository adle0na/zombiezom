using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [LabelText("문 이미지")]
    public SpriteRenderer doorSprite;

    [LabelText("문 데이터")]
    public DoorData doorData;

    [LabelText("생성된 박스 프리팹")]
    public Box boxObj;
    
    PlayerInteract interact;

    public void ApplySpriteByType(Sprite setSprite)
    {
        if (doorSprite == null)
        {
            Debug.LogWarning($"{name}: doorSprite(SPR) 참조가 없습니다.");
            return;
        }
        if (doorData == null)
        {
            Debug.LogWarning($"{name}: doorData가 null 입니다.");
            return;
        }

        doorSprite.sprite = setSprite;
    }

    private void Start()
    {
        interact = FindAnyObjectByType<PlayerInteract>();
    }

    public IInteractable.InteractHoldType HoldType => interact.IsHiding ? IInteractable.InteractHoldType.Instant : IInteractable.InteractHoldType.Cabinet;
    public bool IsInteractable => doorData.isOpenable; 
    public void Interact()
    {
        interact.InteractDoor(this);
    }

    public string GetInteractPrompt()
    {
        return interact.IsHiding ? "나가기" : "숨기";
    }
}
