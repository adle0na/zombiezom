using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    void Start()
    {
        ResolutionScaler.ApplyTargetResolution();
        SoundManager.Instance.PlayBGM(0);
    }
    
    public void GotoStoryScene()
    {
        SceneManager.LoadScene(1);
    }
}
