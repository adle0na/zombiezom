using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeSceneController : MonoBehaviour
{
    public void GotoMapScene()
    {
        // 페이드 인 실행
        UIManager.Instance.OpenFadeInUI();
        // 1초 뒤 씬 이동
        StartCoroutine(LoadSceneWithDelay(2, 1.6f));
    }

    private IEnumerator LoadSceneWithDelay(int sceneIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex);
    }
}
