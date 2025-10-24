using UnityEngine;
using UnityEngine.UI;

public class ApartSceneController : MonoBehaviour
{
    public GameObject imagePrefab;   // 복제할 이미지 프리팹
    public Transform parent;         // 이미지들이 배치될 부모 (예: Canvas 안의 Panel)
    public int columns = 2;          // 가로 개수
    public int rows = 5;             // 세로 개수
    public float spacingX = 200f;    // 가로 간격
    public float spacingY = 200f;    // 세로 간격
    public Vector2 startPos = new Vector2(-100f, 400f); // 첫 이미지의 시작 위치

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

                float posX = startPos.x + x * spacingX;
                float posY = startPos.y - y * spacingY;

                rect.anchoredPosition = new Vector2(posX, posY);
            }
        }
    }
}