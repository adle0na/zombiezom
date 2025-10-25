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
    
    public GameObject SettingPopupPrefab;

    public GameObject fadeUI;
    
    public GameObject getHit;
    public GameObject gameOver;
    
    [SerializeField, LabelText("마스터 캔버스 태그(선택)")]
    private string targetCanvasTag = "MainCanvas"; 
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
        // 모든 씬에서 유지
        DontDestroyOnLoad(gameObject);
    }
    
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

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Escape))
    //     {
    //         if (CurrentUIStatus == CurrentUIStatus.ABBPopup)
    //         {
    //             if (ABBAction == null)
    //             {
    //                 PopupListPop();
    //             }
    //             else
    //             {
    //                 ABBAction();
    //             }
    //         }
    //             
    //     }
    // }
    
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

    #region UIFunctions

    #endregion
    
    #region PopupFunctions

    // ABB 입력 액션을 가진 팝업 생성
    public void PopupListAddABB(GameObject gameObject, Action action)
    {
        PopupList.Add(gameObject);

        gameObject.SetActive(true);
        
        CurrentPopup = gameObject;

        DarkBGCheck();
    }
    
    // ABB 입력 액션이 없는 팝업 생성
    public void PopupListAddNoneABB(GameObject gameObject)
    {
        PopupList.Add(gameObject);

        gameObject.SetActive(true);
        
        CurrentPopup = gameObject;

        DarkBGCheck();
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

        DarkBGCheck();
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
        CurrentPopup = null;

        DarkBGCheck();
    }
    
    // 팝업이 한개라도 있는 경우 DargBG를 통해 UI 터치를 막음
    private void DarkBGCheck()
    {
        popupParent.gameObject.SetActive(PopupList.Count > 0);
    }

    public void OpenPopup(GameObject popupObj)
    {
        GameObject Popup = Instantiate(popupObj, popupParent);
        
        PopupListAddABB(Popup, null);
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
    
    #endregion
}