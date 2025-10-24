using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeSceneController : MonoBehaviour
{
    public void GotoMapScene()
    {
        SceneManager.LoadScene(2);
    }
}
