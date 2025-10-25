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

        float currentY = startPos.y;

        foreach (var data in floorDatas)
        {
            GameObject floor = Instantiate(floorPrefab, transform);
            floor.name = $"Floor_{data.floorValue}_{data.floorType}";

            // 위치 배치
            var sr = floor.GetComponent<SpriteRenderer>();
            float height = (sr != null) ? sr.bounds.size.y : 1f;
            floor.transform.position = new Vector3(startPos.x, currentY, 0f);
            currentY += height + gapY;

            // 타입에 따라 스프라이트 지정
            var getFloor = floor.GetComponent<ApartmentFloor>();
            if (getFloor != null)
            {
                if (data.floorValue == 1)            getFloor.SetFloor(bottomStair);
                else if (data.floorValue == floorValue) getFloor.SetFloor(topStair);
                else                                   getFloor.SetFloor(middleStair);

                // ✅ 벽 컬러 그라데이션 적용
                getFloor.SetWallsGradient(targetColor, data.floorValue, floorValue);
            }
            else
            {
                Debug.LogWarning($"ApartmentFloor 컴포넌트가 {floor.name}에 없습니다.");
            }
            
            floors.Add(getFloor);
        }

        // 랜덤 문 값 적용
        foreach (var floor in floors)
        {
            SetDoor(floor.doorDatas);

            SetDoorSprite(floor.doorDatas);
        }
        
        // 아이템 배치
        
        //Instantiate(playerPrefab)
    }

    public void SetDoor(List<Door> doors)
    {
        if (doors == null || doors.Count == 0)
        {
            Debug.LogWarning($"{name}: doorDatas 비어 있음");
            return;
        }

        // 1개를 랜덤으로 뽑아 '닫힌 판자문'으로 지정
        int lockedIndex = Random.Range(0, doors.Count);

        // 일반/피묻은 문 타입 후보
        DoorType[] openablePool =
        {
            DoorType.NormalDoorA,
            DoorType.NormalDoorB,
            DoorType.BloodDoorA,
            DoorType.BloodDoorB,
            DoorType.BloodDoorC
        };

        for (int i = 0; i < doors.Count; i++)
        {
            var door = doors[i];
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
            
            // 박스 생성 확률 50%
            door.doorData.hasBox = Random.value < 0.5f;

            if (door.doorData.hasBox)
            {
                bool isLeft = Random.value < 0.5f;

                door.boxObj = Instantiate(boxPrefab, isLeft ? door.doorData.leftBoxPos : door.doorData.rightBoxPos).GetComponent<Box>();
            }
            
            // 일반/피묻은 문 타입 후보
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
            
            var bt = boxPool[Random.Range(0, boxPool.Length)];
            door.doorData.boxData.boxType = bt;

            if (door.doorData.hasBox)
            {
                switch (door.doorData.boxData.boxType)
                {
                    case BoxType.NormalBox_S:
                        door.boxObj.boxSprite.sprite = normalBox_S;
                        break;
                    case BoxType.DirtyBox_S:
                        door.boxObj.boxSprite.sprite = dirtyBox_S;
                        break;
                    case BoxType.BloodBox_S:
                        door.boxObj.boxSprite.sprite = bloodBox_S;
                        break;
                    case BoxType.CrumpledBox_S:
                        door.boxObj.boxSprite.sprite = crumbledBox_S;
                        break;
                    case BoxType.NormalBox_L:
                        door.boxObj.boxSprite.sprite = normalBox_L;
                        break;
                    case BoxType.BloodBox_L:
                        door.boxObj.boxSprite.sprite = bloodBox_L;
                        break;
                    case BoxType.CrumpledBox_L:
                        door.boxObj.boxSprite.sprite = crumbledBox_L;
                        break;
                }
            }
        }
    }

    public void SetDoorSprite(List<Door> doors)
    {
        foreach (var door in doors)
        {
            switch (door.doorData.doorType)
            {
                case DoorType.BloodDoorA:
                    door.ApplySpriteByType(bloodDoorA);
                    break;
                case DoorType.BloodDoorB:
                    door.ApplySpriteByType(bloodDoorB);
                    break;
                case DoorType.BloodDoorC:
                    door.ApplySpriteByType(bloodDoorC);
                    break;
                case DoorType.ClosedDoorA:
                    door.ApplySpriteByType(closedDoorA);
                    break;
                case DoorType.ClosedDoorB:
                    door.ApplySpriteByType(closedDoorB);
                    break;
                case DoorType.NormalDoorA:
                    door.ApplySpriteByType(normalDoorA);
                    break;
                case DoorType.NormalDoorB:
                    door.ApplySpriteByType(normalDoorB);
                    break;
            }
        }
    }
}
