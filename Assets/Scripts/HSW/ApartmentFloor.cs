using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

[Serializable]
public class ApartmentFloor : MonoBehaviour
{
    [LabelText("계단")]
    [SerializeField] private SpriteRenderer floorImage;

    [LabelText("1층 집으로 그래피티")]
    [SerializeField] private GameObject homeImage;
    
    [LabelText("칸 이미지 값")]
    [SerializeField] private List<SpriteRenderer> walls;
    
    [LabelText("문 정보")]
    public List<Door> doorDatas;

    [LabelText("세팅된 아이템")]
    public List<ItemCsvRow> settedItems;

    [LabelText("배치된 박스 리스트")]
    public List<Box> settedBox;

    public int floorNum;

    // 층 세팅 함수
    public void SetFloor(Sprite floorSprite)
    {
        floorImage.sprite = floorSprite;
    }

    public void SetHome(bool hasHome)
    {
        homeImage.gameObject.SetActive(hasHome);
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
        
        foreach (var d in doorDatas)
        {
            if (!d.doorSprite) continue;
            float a = d.doorSprite.color.a;
            d.doorSprite.color = new Color(resultColor.r, resultColor.g, resultColor.b, a);
        }
    }
    
    // 박스 생성처리
    public void SetBox()
    {
        if (settedItems == null || settedItems.Count == 0 || settedBox == null || settedBox.Count == 0)
            return;

        foreach (var item in settedItems)
        {
            Box targetBox = null;
            int safety = 0; // 무한루프 방지

            // 3개 미만인 박스를 찾을 때까지 반복
            while (safety < 10)
            {
                int randIndex = UnityEngine.Random.Range(0, settedBox.Count);
                var box = settedBox[randIndex];
                safety++;

                if (box == null) continue;

                // BoxData 초기화
                if (box.boxData == null)
                    box.boxData = new BoxData();
                if (box.boxData.boxItems == null)
                    box.boxData.boxItems = new List<ItemCsvRow>();

                if (box.boxData.boxType == BoxType.CatBox_S)
                    continue;
                
                // 3개 미만이면 사용 가능
                if (box.boxData.boxItems.Count < 3)
                {
                    targetBox = box;
                    break;
                }
            }

            // 조건을 만족한 박스가 있으면 아이템 추가
            if (targetBox != null)
            {
                targetBox.boxData.boxItems.Add(item);
                Debug.Log($"아이템 [{item.itemName}]이 {targetBox.name}에 추가됨 (현재 {targetBox.boxData.boxItems.Count}개)");
            }
            else
            {
                Debug.LogWarning("⚠️ 3개 미만인 박스를 찾지 못했습니다. 아이템을 추가하지 못했습니다.");
            }
        }
    }

    // 간단 셔플 헬퍼
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}