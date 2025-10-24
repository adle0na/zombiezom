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
    
    [LabelText("문 정보")]
    public List<Door> doorDatas;
    
    // 층 세팅 함수
    public void SetFloor(Sprite floorSprite)
    {
        floorImage.sprite = floorSprite;
    }

    public void SetDoor(int minDoor /* 사용 계획 있으면 활용 */)
    {
        if (doorDatas == null || doorDatas.Count == 0)
        {
            Debug.LogWarning($"{name}: doorDatas 비어 있음");
            return;
        }

        // 1개를 랜덤으로 뽑아 '닫힌 판자문'으로 지정
        int lockedIndex = UnityEngine.Random.Range(0, doorDatas.Count);

        // 일반/피묻은 문 타입 후보
        DoorType[] openablePool =
        {
            DoorType.NormalDoorA,
            DoorType.NormalDoorB,
            DoorType.BloodDoorA,
            DoorType.BloodDoorB,
            DoorType.BloodDoorC
        };

        for (int i = 0; i < doorDatas.Count; i++)
        {
            var door = doorDatas[i];
            if (door == null) continue;
            
            if (i == lockedIndex)
            {
                // 닫힌 판자문(A/B) 50:50
                bool pickA = UnityEngine.Random.value < 0.5f;
                door.doorData.doorType = pickA ? DoorType.ClosedDoorA : DoorType.ClosedDoorB;
                door.doorData.isOpenable = false;
            }
            else
            {
                // 나머지는 5종 중 랜덤 + 열림
                var t = openablePool[UnityEngine.Random.Range(0, openablePool.Length)];
                door.doorData.doorType = t;
                door.doorData.isOpenable = true;
            }
            
            // door.ApplySpriteByType();
        }
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