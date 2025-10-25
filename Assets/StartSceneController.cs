using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    void Start()
    {
        SoundManager.Instance.PlayBGM(0);
    }
    
    public void GotoHomeScene()
    {
        SceneManager.LoadScene(1);
    }
}
