using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ApartSceneController : MonoBehaviour
{
    [LabelText("배경 이미지")]
    [SerializeField] private GameObject imagePrefab;
    [LabelText("스폰 위치")]
    [SerializeField] private Transform parent;
    
    // 아파트는 2칸으로 고정
    private int columns = 2;
    
    [LabelText("아파트 층 높이")]
    [SerializeField] private int rows;

    // 이미지 가로 세로 값
    private float spacingX;
    private float spacingY;
    
    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject img = Instantiate(imagePrefab, parent);
                RectTransform rect = img.GetComponent<RectTransform>();

                float posX = parent.position.x + x * spacingX;
                float posY = parent.position.y - y * spacingY;

                rect.anchoredPosition = new Vector2(posX, posY);
            }
        }
    }
}