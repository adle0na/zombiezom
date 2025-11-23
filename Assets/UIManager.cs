using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : GenericSingleton<UIManager>
{
    private static UIManager instance;
    
    // SceneController (Canvas)
    public Canvas sceneController;

    // 팝업 생성 위치
    public Transform popupParent;

    // Canvas에 존재하는 PopupList
    public List<GameObject> PopupList;

    // 가장 최근에 생성된 팝업
    public GameObject CurrentPopup;
    
    [Header("PopupPrefabs")]
    public GameObject popupParentPrefab;
    
    public GameObject fadeUI;
    
    public GameObject getHit;
    public GameObject gameOver;
    public GameObject settingPopup;
    public GameObject infoPopup;
    
    [SerializeField, LabelText("마스터 캔버스 태그(선택)")]
    private string targetCanvasTag = "MainCanvas"; 
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCanvasAndUIController();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 이동시 마다 컨트롤러 세팅
    public void FindCanvasAndUIController()
    {
        sceneController = null;
        
        if (!string.IsNullOrEmpty(targetCanvasTag))
        {
            var go = GameObject.FindGameObjectWithTag(targetCanvasTag);
            if (go) sceneController = go.GetComponent<Canvas>();
            if (sceneController == null && go != null)
                Debug.LogWarning($"[{name}] 태그 '{targetCanvasTag}' 객체에 Canvas 컴포넌트가 없습니다: {go.name}");
        }
        
        if (sceneController != null)
        {
            GameObject popupTransformObj = Instantiate(popupParentPrefab, sceneController.transform);

            popupTransformObj.transform.SetSiblingIndex(sceneController.transform.childCount - 1);

            popupParent = popupTransformObj.transform;
        }
        else
        {
            Debug.LogError("UI 컨트롤러 탐색 실패");
        }
    }
    
    // 맨위의 팝업 종료
    public void PopupListPop()
    {
        if (PopupList != null && PopupList.Count > 0)
        {
            if (CurrentPopup != null && CurrentPopup.activeSelf)
            {
                Destroy(CurrentPopup);
                    
                PopupList.RemoveAt(PopupList.Count - 1);

                if (PopupList.Count > 0)
                {
                    CurrentPopup = PopupList[0];
                }
            }
        }
    }

    // UI 전환시 모든 팝업 종료
    public void AllPopupClear()
    {
        foreach (GameObject popup in PopupList)
        {
            if (popup != null && popup.activeSelf)
            {
                Destroy(popup);
            }
        }
        PopupList.Clear();
        
        Destroy(CurrentPopup);
        
        CurrentPopup = null;

        Time.timeScale = 1f;

    }
    
    public void OpenFadeInUI()
    {
        GameObject getUI = Instantiate(fadeUI, popupParent);
        getUI.GetComponent<FadeUI>().FadeIn(Color.black);
    }

    public void OpenFadeOutUI()
    {
        GameObject getUI = Instantiate(fadeUI, popupParent);
        getUI.GetComponent<FadeUI>().FadeOut(Color.black);
    }

    public void OpenBiteUI()
    {
        GameObject getUI = Instantiate(getHit, popupParent);
    }

    public void OpenDeadUI()
    {
        GameObject getUI = Instantiate(gameOver, popupParent);

        PlayerDataManager.Instance.ResetData();
    }

    public void OpenSettingPopup()
    {
        GameObject getUI = Instantiate(settingPopup, popupParent);

        CurrentPopup = getUI;
        
        Time.timeScale = 0f;
    }
    
    public void OpenInfoPopup()
    {
        GameObject getUI = Instantiate(infoPopup, popupParent);

        CurrentPopup = getUI;
        
        Time.timeScale = 0f;
    }
}