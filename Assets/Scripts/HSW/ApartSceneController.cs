using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ApartSceneController : MonoBehaviour
{
    [LabelText("층수")]
    [SerializeField] private int floorValue;

    [HorizontalGroup("Split", 0.5f)]
    [Button("층 데이터 생성", ButtonSizes.Large), GUIColor(0, 1f, 0)]
    private void CreateFloorData()
    {
        floorDatas.Clear();
        for (int i = 1; i <= floorValue; i++)
        {
            FloorData getFloorData = new FloorData();
            getFloorData.floorValue = i;

            if (i == 1)
                getFloorData.floorType = FloorType.Bottom;
            else if (i == floorValue)
                getFloorData.floorType = FloorType.Top;
            else
                getFloorData.floorType = FloorType.Middle;

            floorDatas.Add(getFloorData);
        }
    }

    [VerticalGroup("Split/right")]
    [Button("층 데이터 초기화", ButtonSizes.Large), GUIColor(1f, 0, 0)]
    private void ResetFloorData()
    {
        floorDatas.Clear();
    }

    [LabelText("층별 데이터")]
    [SerializeField] private List<FloorData> floorDatas;

    [LabelText("생성된 층 정보")]
    [SerializeField] private List<ApartmentFloor> floors;

    [LabelText("최소 열리는문")]
    [SerializeField] private int minOpenDoor;
    
    [Title("맵 리소스")]
    [LabelText("중앙")] [SerializeField] private Sprite middleStair;
    [LabelText("상단")] [SerializeField] private Sprite topStair;
    [LabelText("하단")] [SerializeField] private Sprite bottomStair;

    [LabelText("층 프리팹")]
    [SerializeField] private GameObject floorPrefab;

    [LabelText("박스 프리팹")]
    [SerializeField] private GameObject boxPrefab;

    [LabelText("플레이어 프리팹")]
    [SerializeField] private GameObject playerPrefab;

    [LabelText("생성된 플레이어")]
    [SerializeField] private GameObject playerObj;

    [LabelText("판자문A")] 
    [SerializeField] private Sprite closedDoorA;
    [LabelText("판자문B")] 
    [SerializeField] private Sprite closedDoorB;
    
    [LabelText("일반문A")] 
    [SerializeField] private Sprite normalDoorA;
    [LabelText("일반문B")] 
    [SerializeField] private Sprite normalDoorB;

    [LabelText("피묻은문A")] 
    [SerializeField] private Sprite bloodDoorA;
    [LabelText("피묻은문B")] 
    [SerializeField] private Sprite bloodDoorB;
    [LabelText("피묻은문C")] 
    [SerializeField] private Sprite bloodDoorC;
    
    [LabelText("작은 일반 박스")] 
    [SerializeField] private Sprite normalBox_S;
    [LabelText("작은 더러운 박스")] 
    [SerializeField] private Sprite dirtyBox_S;
    [LabelText("작은 찌그러진 박스")] 
    [SerializeField] private Sprite crumbledBox_S;
    [LabelText("작은 피묻은 박스")] 
    [SerializeField] private Sprite bloodBox_S;

    [LabelText("큰 일반 박스")] 
    [SerializeField] private Sprite normalBox_L;
    [LabelText("큰 찌그러진 박스")] 
    [SerializeField] private Sprite crumbledBox_L;
    [LabelText("큰 피묻은 박스")] 
    [SerializeField] private Sprite bloodBox_L;

    [LabelText("레이어 컬러")]
    [SerializeField] private Color targetColor;
    
    // 층 간 간격(Y축)
    private float gapY = 4f;

    // 생성 시작 위치 (월드 좌표)
    private Vector2 startPos = new Vector2(0f, 0f);

    void Start()
    {
        CreateMapWithFloorData();
    }

    void CreateMapWithFloorData()
    {
        if (floorPrefab == null)
        {
            Debug.LogError("❌ floorPrefab이 설정되지 않았습니다.");
            return;
        }
        if (floorDatas == null || floorDatas.Count == 0)
        {
            Debug.LogWarning("⚠️ floorDatas가 비어 있습니다. CreateFloorData()를 먼저 실행하세요.");
            return;
        }

        if (floors == null) floors = new List<ApartmentFloor>();
        else floors.Clear();
        
        float currentY = startPos.y;

        foreach (var data in floorDatas)
        {
            GameObject floorGo = Instantiate(floorPrefab, transform);
            floorGo.name = $"Floor_{data.floorValue}_{data.floorType}";

            // 위치 배치 높이 계산은 스프라이트 기준 (자식 포함 안전)
            var sr = floorGo.GetComponent<SpriteRenderer>();
            if (sr == null) sr = floorGo.GetComponentInChildren<SpriteRenderer>(true);
            float height = (sr != null) ? sr.bounds.size.y : 1f;

            floorGo.transform.position = new Vector3(startPos.x, currentY, 0f);
            currentY += height + gapY;

            var floor = floorGo.GetComponent<ApartmentFloor>();
            if (floor != null)
            {
                // 층 스프라이트
                if (data.floorValue == 1)                 floor.SetFloor(bottomStair);
                else if (data.floorValue == floorValue)   floor.SetFloor(topStair);
                else                                      floor.SetFloor(middleStair);

                // 그라데이션
                floor.SetWallsGradient(targetColor, data.floorValue, floorValue);

                // // 문 타입(닫힌 1개 + 나머지 랜덤)
                // SetDoor(floor.doorDatas);
                //
                // // 문 스프라이트 반영
                // SetDoorSprite(floor.doorDatas);

                // ✅ 이 층의 itemList 기준으로 박스 & 아이템 배치
                DistributeBoxesAndItems(floor, data);
            }
            else
            {
                Debug.LogWarning($"ApartmentFloor 컴포넌트가 {floorGo.name}에 없습니다.");
            }

            floors.Add(floor);
        }

        // 랜덤 문 값 적용
        // foreach (var floor in floors)
        // {
        //     SetDoor(floor.doorDatas);
        //
        //     SetDoorSprite(floor.doorDatas);
        // }
        
        // 아이템 배치
        
        playerObj = Instantiate(playerPrefab);
    }

    private void DistributeBoxesAndItems(ApartmentFloor floor, FloorData data)
    {
        if (floor == null || floor.doorDatas == null || floor.doorDatas.Count == 0) return;

        var items = (data.itemList != null) ? new List<int>(data.itemList) : new List<int>();
        int n = items.Count;

        // 목표 박스 수
        int targetBoxes = n / 2 + 1;

        // 문당 1개만 둘 수 있으므로 상한 = 문 개수
        int maxPlaceable = floor.doorDatas.Count;
        if (targetBoxes > maxPlaceable) targetBoxes = maxPlaceable;
        if (targetBoxes <= 0) return;

        // 문 목록 섞기 (박스 놓을 문 랜덤 선택)
        var doors = new List<Door>(floor.doorDatas);
        Shuffle(doors);

        // 박스 타입 후보
        BoxType[] boxPool =
        {
            BoxType.NormalBox_S,
            BoxType.NormalBox_L,
            BoxType.DirtyBox_S,
            BoxType.CrumpledBox_S,
            BoxType.CrumpledBox_L,
            BoxType.BloodBox_S,
            BoxType.BloodBox_L
        };

        // 생성된 박스 모음
        var createdBoxes = new List<Box>(targetBoxes);

        // 1) 박스 생성: 랜덤 문에 1개씩
        int created = 0;
        for (int i = 0; i < doors.Count && created < targetBoxes; i++)
        {
            var door = doors[i];
            if (door == null || door.doorData == null) continue;

            // 이미 박스가 있다면(다른 로직에서 만들었을 수 있음) 스킵
            if (door.boxObj != null) continue;

            // 왼/오 위치 중 하나 랜덤 (없으면 문 위치 기준)
            Transform parent = null;
            bool isLeft = Random.value < 0.5f;
            if (isLeft && door.doorData.leftBoxPos != null) parent = door.doorData.leftBoxPos;
            else if (!isLeft && door.doorData.rightBoxPos != null) parent = door.doorData.rightBoxPos;
            else parent = door.transform;

            // 프리팹 생성
            var boxGo = Instantiate(boxPrefab, parent.position, Quaternion.identity, parent);
            var box = boxGo.GetComponent<Box>();
            if (box == null)
            {
                Debug.LogWarning($"{boxGo.name}: Box 컴포넌트가 없습니다.");
                continue;
            }

            // 박스 타입 랜덤 선정 + 스프라이트 반영
            var bt = boxPool[Random.Range(0, boxPool.Length)];
            door.doorData.hasBox = true;
            door.doorData.boxData.boxType = bt;
            SetBoxSpriteByType(box, bt);

            // 문이 소유한 박스 참조 연결(문당 1개 제한 구조)
            door.boxObj = box;

            createdBoxes.Add(box);
            created++;
        }

        // 2) 아이템 무작위 배치 (박스당 0~3개)
        if (items.Count == 0 || createdBoxes.Count == 0) return;

        const int BoxCapacity = 3;

        // 아이템 섞기
        Shuffle(items);

        // 각 박스의 현재 적재 수
        var fill = new int[createdBoxes.Count];

        // 라운드-로빈에 랜덤을 섞은 분배: 아이템을 차례로 박스에 시도하되, 꽉 찬 박스는 건너뜀
        int boxIndex = Random.Range(0, createdBoxes.Count);
        foreach (var itemId in items)
        {
            // 모든 박스가 꽉 찼으면 중단
            bool allFull = true;
            for (int k = 0; k < createdBoxes.Count; k++)
            {
                if (fill[k] < BoxCapacity) { allFull = false; break; }
            }
            if (allFull) break;

            // 빈 슬롯 있는 박스를 만날 때까지 순환
            int tries = 0;
            while (fill[boxIndex] >= BoxCapacity && tries < createdBoxes.Count)
            {
                boxIndex = (boxIndex + 1) % createdBoxes.Count;
                tries++;
            }
            if (fill[boxIndex] >= BoxCapacity)
                continue; // 안전

            // 아이템 적재
            var box = createdBoxes[boxIndex];
            if (box.boxData.boxItems == null) box.boxData.boxItems = new List<ItemCsvRow>();

            foreach (var getData in ItemDataManager.Instance.itemList)
            {
                if (getData.index == boxIndex)
                {
                    box.boxData.boxItems.Add(getData);
                }
            }
            
            fill[boxIndex]++;

            // 다음 박스로 이동 (랜덤성 조금 더)
            boxIndex = (boxIndex + Random.Range(1, createdBoxes.Count)) % createdBoxes.Count;
        }
    }

    private void SetBoxSpriteByType(Box box, BoxType type)
    {
        if (box == null || box.boxSprite == null) return;

        switch (type)
        {
            case BoxType.NormalBox_S:   box.boxSprite.sprite = normalBox_S;   break;
            case BoxType.DirtyBox_S:    box.boxSprite.sprite = dirtyBox_S;    break;
            case BoxType.BloodBox_S:    box.boxSprite.sprite = bloodBox_S;    break;
            case BoxType.CrumpledBox_S: box.boxSprite.sprite = crumbledBox_S; break;
            case BoxType.NormalBox_L:   box.boxSprite.sprite = normalBox_L;   break;
            case BoxType.BloodBox_L:    box.boxSprite.sprite = bloodBox_L;    break;
            case BoxType.CrumpledBox_L: box.boxSprite.sprite = crumbledBox_L; break;
        }
    }

    // 단순 셔플
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    
    // public void SetDoor(List<Door> doors)
    // {
    //     if (doors == null || doors.Count == 0)
    //     {
    //         Debug.LogWarning($"{name}: doorDatas 비어 있음");
    //         return;
    //     }
    //
    //     // 1개를 랜덤으로 뽑아 '닫힌 판자문'으로 지정
    //     int lockedIndex = Random.Range(0, doors.Count);
    //
    //     // 일반/피묻은 문 타입 후보
    //     DoorType[] openablePool =
    //     {
    //         DoorType.NormalDoorA,
    //         DoorType.NormalDoorB,
    //         DoorType.BloodDoorA,
    //         DoorType.BloodDoorB,
    //         DoorType.BloodDoorC
    //     };
    //
    //     for (int i = 0; i < doors.Count; i++)
    //     {
    //         var door = doors[i];
    //         if (door == null) continue;
    //         
    //         if (i == lockedIndex)
    //         {
    //             // 닫힌 판자문(A/B) 50:50
    //             bool pickA = UnityEngine.Random.value < 0.5f;
    //             door.doorData.doorType = pickA ? DoorType.ClosedDoorA : DoorType.ClosedDoorB;
    //             door.doorData.isOpenable = false;
    //         }
    //         else
    //         {
    //             // 나머지는 5종 중 랜덤 + 열림
    //             var t = openablePool[UnityEngine.Random.Range(0, openablePool.Length)];
    //             door.doorData.doorType = t;
    //             door.doorData.isOpenable = true;
    //         }
    //         
    //         // 박스 생성 확률 50%
    //         door.doorData.hasBox = Random.value < 0.5f;
    //
    //         if (door.doorData.hasBox)
    //         {
    //             bool isLeft = Random.value < 0.5f;
    //
    //             door.boxObj = Instantiate(boxPrefab, isLeft ? door.doorData.leftBoxPos : door.doorData.rightBoxPos).GetComponent<Box>();
    //         }
    //         
    //         // 일반/피묻은 문 타입 후보
    //         BoxType[] boxPool =
    //         {
    //             BoxType.NormalBox_S,
    //             BoxType.NormalBox_L,
    //             BoxType.DirtyBox_S,
    //             BoxType.CrumpledBox_S,
    //             BoxType.CrumpledBox_L,
    //             BoxType.BloodBox_S,
    //             BoxType.BloodBox_L
    //         };
    //         
    //         var bt = boxPool[Random.Range(0, boxPool.Length)];
    //         door.doorData.boxData.boxType = bt;
    //
    //         if (door.doorData.hasBox)
    //         {
    //             switch (door.doorData.boxData.boxType)
    //             {
    //                 case BoxType.NormalBox_S:
    //                     door.boxObj.boxSprite.sprite = normalBox_S;
    //                     break;
    //                 case BoxType.DirtyBox_S:
    //                     door.boxObj.boxSprite.sprite = dirtyBox_S;
    //                     break;
    //                 case BoxType.BloodBox_S:
    //                     door.boxObj.boxSprite.sprite = bloodBox_S;
    //                     break;
    //                 case BoxType.CrumpledBox_S:
    //                     door.boxObj.boxSprite.sprite = crumbledBox_S;
    //                     break;
    //                 case BoxType.NormalBox_L:
    //                     door.boxObj.boxSprite.sprite = normalBox_L;
    //                     break;
    //                 case BoxType.BloodBox_L:
    //                     door.boxObj.boxSprite.sprite = bloodBox_L;
    //                     break;
    //                 case BoxType.CrumpledBox_L:
    //                     door.boxObj.boxSprite.sprite = crumbledBox_L;
    //                     break;
    //             }
    //         }
    //     }
    // }
    //
    // public void SetDoorSprite(List<Door> doors)
    // {
    //     foreach (var door in doors)
    //     {
    //         switch (door.doorData.doorType)
    //         {
    //             case DoorType.BloodDoorA:
    //                 door.ApplySpriteByType(bloodDoorA);
    //                 break;
    //             case DoorType.BloodDoorB:
    //                 door.ApplySpriteByType(bloodDoorB);
    //                 break;
    //             case DoorType.BloodDoorC:
    //                 door.ApplySpriteByType(bloodDoorC);
    //                 break;
    //             case DoorType.ClosedDoorA:
    //                 door.ApplySpriteByType(closedDoorA);
    //                 break;
    //             case DoorType.ClosedDoorB:
    //                 door.ApplySpriteByType(closedDoorB);
    //                 break;
    //             case DoorType.NormalDoorA:
    //                 door.ApplySpriteByType(normalDoorA);
    //                 break;
    //             case DoorType.NormalDoorB:
    //                 door.ApplySpriteByType(normalDoorB);
    //                 break;
    //         }
    //     }
    // }
}
