using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneController : MonoBehaviour
{
    private void Start()
    {
        SoundManager.Instance.PlayBGM(4);
    }

    public void GotoStartScene()
    {
        SceneManager.LoadScene(1);
    }
}
