using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadUI : MonoBehaviour
{
    private void Start()
    {
        Invoke("BackToMain", 0.7f);
    }

    private void BackToMain()
    {
        SceneManager.LoadScene(0);
    }
}
