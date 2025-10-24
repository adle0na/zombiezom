using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ApartmentFloor : MonoBehaviour
{
    [LabelText("계단")]
    [SerializeField]
    private SpriteRenderer floorImage;

    [LabelText("칸 이미지 값")]
    [SerializeField] private List<SpriteRenderer> walls;
    
    [LabelText("문 정보")] [SerializeField]
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
    
    public void SetWallsGradient(Color targetColor, int floorIndex, int maxFloor)
    {
        if (maxFloor <= 1) maxFloor = 1;

        // 1층 → 흰색(1,1,1)
        // N층 → targetColor
        float t = (float)(floorIndex - 1) / (maxFloor - 1); // 0~1 사이 값
        Color startColor = Color.white;

        // 하얀색 → targetColor 방향으로 보간
        Color resultColor = Color.Lerp(startColor, targetColor, t);

        foreach (var w in walls)
        {
            if (!w) continue;
            float a = w.color.a;
            w.color = new Color(resultColor.r, resultColor.g, resultColor.b, a);
        }
    }
}