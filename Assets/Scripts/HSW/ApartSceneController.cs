using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [LabelText("일반좀비")] [SerializeField] private GameObject normalZombie;
    [LabelText("치료불가좀비")] [SerializeField] private GameObject discureZombie;
    [LabelText("수아좀비")] [SerializeField] private GameObject suaZombie;

    [LabelText("홈 UI")] [SerializeField] private GameObject homeUI;

    [LabelText("레이어 컬러")]
    [SerializeField] private Color targetColor;
    
    // 층 간 간격(Y축)
    private float gapY = 4f;

    // 생성 시작 위치 (월드 좌표)
    private Vector2 startPos = new Vector2(0f, 0f);

    void Start()
    {
        UIManager.Instance.OpenFadeOutUI();
        
        CreateMapWithFloorData();
        
        SoundManager.Instance.PlayBGM(2);
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

            //data.itemList = new List<int>()
            
            var floor = floorGo.GetComponent<ApartmentFloor>();
            if (floor != null)
            {
                // 층 스프라이트
                if (data.floorValue == 1)                 floor.SetFloor(bottomStair);
                else if (data.floorValue == floorValue)   floor.SetFloor(topStair);
                else                                      floor.SetFloor(middleStair);
             
                // 1층만 집 표기
                floor.SetHome(data.floorValue == 1);
                
                // 그라데이션
                floor.SetWallsGradient(targetColor, data.floorValue, floorValue);

                // 문 타입(닫힌 1개 + 나머지 랜덤)
                SetDoor(floor.doorDatas);
                
                // 문 스프라이트 반영
                SetDoorSprite(floor.doorDatas);

                for (int i = 0; i < data.itemList.Count; i++)
                {
                    floor.settedItems.Add(ItemDataManager.Instance.GetItemByIndex(data.itemList[i]));
                }
                
                // ✅ 이 층의 itemList 기준으로 박스 & 아이템 배치
                DistributeBoxesAndItems(floor, data);
                
                // 좀비 배치
                SpawnZombies(floor, data);
                
                floor.floorNum = data.floorValue;
            }
            else
            {
                Debug.LogWarning($"ApartmentFloor 컴포넌트가 {floorGo.name}에 없습니다.");
            }

            floors.Add(floor);
        }
        
        foreach (var floor in floors)
        {
            SetDoor(floor.doorDatas);
        
            SetDoorSprite(floor.doorDatas);
        }
        
        // 플레이어 배치
        playerObj = Instantiate(playerPrefab);

        PlayerDataManager.Instance.playerObj = playerObj;
    }

    private void DistributeBoxesAndItems(ApartmentFloor floor, FloorData data)
    {
        if (floor == null || floor.doorDatas == null || floor.doorDatas.Count == 0) return;
    
        // 1) 목표 박스 수
        var idList = (data.itemList != null) ? new List<int>(data.itemList) : new List<int>();
        int n = idList.Count;
        int targetBoxes = n / 2 + 1;
        if (targetBoxes <= 0) return;

        // 문당 1개 제한
        targetBoxes = Mathf.Min(targetBoxes, floor.doorDatas.Count);
    
        // 2) 박스 놓을 문 랜덤 선정
        var candidateDoors = new List<Door>(floor.doorDatas);
        Shuffle(candidateDoors);
    
        // 박스 타입 랜덤 풀
        BoxType[] boxPool =
        {
            BoxType.NormalBox_S, BoxType.NormalBox_L,
            BoxType.DirtyBox_S,
            BoxType.CrumpledBox_S, BoxType.CrumpledBox_L,
            BoxType.BloodBox_S,    BoxType.BloodBox_L
        };
    
        // 3) 박스 생성
        var createdBoxes = new List<Box>(targetBoxes);
        int created = 0;
    
        for (int i = 0; i < candidateDoors.Count && created < targetBoxes; i++)
        {
            var door = candidateDoors[i];
            if (door == null) continue;
    
            // 이미 자식에 Box가 있으면 재사용
            var existingBox = door.GetComponentInChildren<Box>(true);
            Box box;
            if (existingBox != null)
            {
                box = existingBox;
            }
            else
            {
                // 문 좌/우 포인트가 있으면 그 위치, 없으면 문 위치
                Transform parent = door.transform;
                bool useLeft = Random.value < 0.5f;
    
                var leftPos  = (door.doorData != null) ? door.doorData.leftBoxPos  : null;
                var rightPos = (door.doorData != null) ? door.doorData.rightBoxPos : null;
                if (useLeft && leftPos != null) parent = leftPos;
                else if (!useLeft && rightPos != null) parent = rightPos;
    
                var boxGo = Instantiate(boxPrefab, parent.position, Quaternion.identity, parent);
                box = boxGo.GetComponent<Box>();
                if (box == null)
                {
                    Debug.LogWarning($"{boxGo.name}: Box 컴포넌트가 없습니다.");
                    Destroy(boxGo);
                    continue;
                }
            }
    
            // BoxData 초기화 + 위치 플래그
            if (box.boxData == null) box.boxData = new BoxData();
            if (box.boxData.boxItems == null) box.boxData.boxItems = new List<ItemCsvRow>();
            box.boxData.isOpened = false; // 새 상자이므로 닫힘
            // 문 데이터와 동기화(선택)
            if (door.doorData != null)
            {
                door.doorData.hasBox = true;
                door.doorData.boxData = box.boxData;
            }
    
            // 랜덤 박스 타입 지정 + 스프라이트 반영
            var bt = boxPool[Random.Range(0, boxPool.Length)];
            box.boxData.boxType = bt;
            SetBoxSpriteByType(box, bt);
    
            createdBoxes.Add(box);
            created++;
            
            floor.settedBox.Add(box);
        }

        floor.SetBox();
    }
    
    private void SpawnZombies(ApartmentFloor floor, FloorData data)
    {
        if (data == null || data.zombieDatas == null || data.zombieDatas.Count == 0) return;
        
        foreach (var zombieData in data.zombieDatas)
        {
            if (zombieData == null) continue;

            GameObject spawnTarget = normalZombie;

            switch (zombieData.zombieType)
            {
                case ZombieType.NormalZombie:
                    spawnTarget = normalZombie;
                    break;
                case ZombieType.DisCureZombie:
                    spawnTarget = discureZombie;
                    break;
                case ZombieType.SuaZombie:
                    spawnTarget = suaZombie;
                    break;
            }
            
            // 스폰 방향
            bool isLeft = zombieData.isLeftSpawn;

            // X 위치 랜덤
            float x = isLeft
                ? Random.Range(-20f, -2f)
                : Random.Range(  2f, 20f);

            // 층 높이 기준 Y
            float y = floor.transform.position.y - 2;
            Vector3 pos = new Vector3(x, y, 0f);

            // 프리팹 생성
            var zombieGo = Instantiate(spawnTarget, pos, Quaternion.identity, this.transform);
            zombieGo.name = $"Zombie_{(isLeft ? "L" : "R")}_{data.floorValue}";

            // 데이터 주입
            var zombieComp = zombieGo.GetComponent<Zombie>();
            if (zombieComp != null)
            {
                // 새 인스턴스 데이터 넣기
                zombieComp.GetType().GetField("zombieData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(zombieComp, zombieData);
            }

            // 방향 보정 (왼쪽에서 오면 오른쪽을 향하도록)
            var sr = zombieGo.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.flipX = isLeft;
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

    public void IntoHome()
    {
        PlayerDataManager.Instance.playerObj.SetActive(false);
        
        homeUI.SetActive(true);
        
        homeUI.GetComponent<HomeUIScript>().InitHomeUI();

        PlayerDataManager.Instance.PlayerInHome();
    }
    
    public void QuitHome()
    {
        homeUI.SetActive(false);
        
        UIManager.Instance.OpenFadeOutUI();
        
        PlayerDataManager.Instance.playerObj.SetActive(true);
    }

    public void BackToMain()
    {
        SceneManager.LoadScene(0);
    }
}
