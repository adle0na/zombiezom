using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadUI : MonoBehaviour
{
    private void Start()
    {
        Invoke("BackToMain", 3f);
    }

    private void BackToMain()
    {
        SceneManager.LoadScene(1);
    }
}
