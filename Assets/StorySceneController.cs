using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneController : MonoBehaviour
{
    public void GotoStartScene()
    {
        SceneManager.LoadScene(1);
    }
}
