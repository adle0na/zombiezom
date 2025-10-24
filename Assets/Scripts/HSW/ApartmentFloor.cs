using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ApartmentFloor : MonoBehaviour
{
    [LabelText("계단")]
    [SerializeField] private SpriteRenderer floorImage;

    [LabelText("칸 정보")] [SerializeField]
    private List<DoorData> doorDatas;
    
    // 층 세팅 함수
    public void SetFloor(Sprite floorSprite)
    {
        floorImage.sprite = floorSprite;
    }

    private void SetDoor()
    {
        // 문생성
        DoorData getDoor = new DoorData();
    }
    
    // 박스 생성처리
    private void SetBox()
    {
        
    }
}
