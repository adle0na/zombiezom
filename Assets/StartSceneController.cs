using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    public void GotoHomeScene()
    {
        SceneManager.LoadScene(1);
    }
}
