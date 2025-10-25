using Sirenix.OdinInspector;
using UnityEngine;

public class Door : MonoBehaviour
{
    [LabelText("문 이미지")]
    public SpriteRenderer doorSprite;

    [LabelText("문 데이터")]
    public DoorData doorData;

    [LabelText("생성된 박스 프리팹")]
    public Box boxObj;

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
}
