using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Image fadeImage;

    private Coroutine fadeRoutine;

    public void FadeIn(Color baseColor)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeImage(0f, 1f, baseColor));
    }

    public void FadeOut(Color baseColor)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeImage(1f, 0f, baseColor));
    }

    private IEnumerator FadeImage(float from, float to, Color color)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(color.r, color.g, color.b, to);
        
        Destroy(gameObject, 0.1f);
    }
}
