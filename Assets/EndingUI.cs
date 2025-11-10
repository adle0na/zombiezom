using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;
    [SerializeField] private GameObject ending1;
    [SerializeField] private GameObject ending2;

    // 스크롤 속도 조절
    [SerializeField] private float scrollTime = 10f;

    private ScrollRect scrollRect;

    private void Awake()
    {
        if (scrollView != null)
            scrollRect = scrollView.GetComponentInChildren<ScrollRect>();
    }

    public void CheckEnding()
    {
        bool isFindCat = PlayerDataManager.Instance.isFindCat;

        ending1.SetActive(isFindCat);
        ending2.SetActive(!isFindCat);

        // 스크롤 자동 진행 시작
        if (scrollRect != null)
            StartCoroutine(AutoScrollRoutine());
        else
            Debug.LogWarning("ScrollRect 컴포넌트를 찾지 못했습니다.");
    }

    private IEnumerator AutoScrollRoutine()
    {
        // 스크롤 위치 맨 위로 초기화
        scrollRect.normalizedPosition = new Vector2(0, 1);

        float t = 0f;
        while (t < scrollTime)
        {
            t += Time.deltaTime;

            // Lerp으로 위(1) → 아래(0) 자연스럽게 이동
            float value = Mathf.Lerp(1f, 0f, t / scrollTime);
            scrollRect.normalizedPosition = new Vector2(0, value);

            yield return null;
        }

        // 혹시 미세하게 덜 내려갔을 경우 보정
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}