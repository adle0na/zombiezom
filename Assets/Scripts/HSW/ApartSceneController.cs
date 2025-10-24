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
    [SerializeField] private List<FloorData> floorDatas = new List<FloorData>();

    [LabelText("층 프리팹")]
    [SerializeField] private GameObject floorPrefab;

    [Title("계단 이미지")]
    [LabelText("중앙")] [SerializeField] private Sprite middleStair;
    [LabelText("상단")] [SerializeField] private Sprite topStair;
    [LabelText("하단")] [SerializeField] private Sprite bottomStair;

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
        }
    }

    private void ApplyFloorTypeVisual(GameObject floor, FloorType type)
    {
        // 필요 시 프리팹에 SpriteRenderer가 있는 경우 스프라이트 교체
        var sr = floor.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (type)
        {
            case FloorType.Top:
                sr.sprite = topStair;
                break;
            case FloorType.Middle:
                sr.sprite = middleStair;
                break;
            case FloorType.Bottom:
                sr.sprite = bottomStair;
                break;
        }
    }
}
