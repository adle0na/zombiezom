using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeUIScript : MonoBehaviour
{
    // 좀비데이터
    public ZombieData target;

    public GameObject normalZombie;
    public GameObject sua;
    public GameObject chair;
    public GameObject quiz;

    private int[] firstFloor = {5, 9};
    private int[] secondFloor = {5, 15};
    private int[] thirdFloor = {20, 16, 5};
    private int[] fourthFloor = {17, 10, 8, 5};
    private int[] fivethFloor = {0, 1, 2, 3, 4, 5};

    public List<int> remainCure = new List<int>();
    public int caringNum;

    public Image item;
    
    public void InitHomeUI()
    {
        foreach (var invenData in PlayerDataManager.Instance.PlayerInventoryData)
        {
            if (invenData.index > 21 && invenData.index < 26)
            {
                normalZombie.SetActive(true);

                caringNum = invenData.index;
                PlayerInventory.Instance.RemoveItemByIndex(invenData.index);
                switch (invenData.index)
                {
                    case 22 :
                        foreach (var cure in firstFloor)
                        {
                            remainCure.Add(cure);
                        }
                        break;
                    case 23 :
                        foreach (var cure in secondFloor)
                        {
                            remainCure.Add(cure);
                        }
                        break;
                    case 24 :
                        foreach (var cure in thirdFloor)
                        {
                            remainCure.Add(cure);
                        }
                        break;
                    case 25 :
                        foreach (var cure in fourthFloor)
                        {
                            remainCure.Add(cure);
                        }
                        break;
                        
                }
            }
            else if (invenData.index == 26)
            {
                PlayerInventory.Instance.RemoveItemByIndex(invenData.index);
                sua.SetActive(true);
                
                foreach (var cure in fivethFloor)
                {
                    remainCure.Add(cure);
                }
            }
        }
        PlayerDataManager.Instance.IsZombieInHome = remainCure.Count > 0;
        UpdateQuiz();
    }

    public void RemoveFirst()
    {
        remainCure.RemoveAt(0);
        UpdateQuiz();
        if (remainCure.Count <= 0)
        {
            // TODO : 인간되는 애니메이션 재생, 딸이라면 엔딩
            Debug.Log("인간되는 애니메이션 재생");
        }
    }

    private void UpdateQuiz()
    {
        if (remainCure.Count > 0)
        {
            quiz.SetActive(true);
            item.sprite = ItemDataManager.Instance.GetItemByIndex(remainCure[0]).itemSprite;
            Debug.Log($"{ItemDataManager.Instance.GetItemByIndex(remainCure[0]).itemSprite}");
        }
        else
        {
            quiz.SetActive(false);
        }
    }

    public void QuitHome()
    {
        PlayerDataManager.Instance.remainCure = remainCure;
        //PlayerDataManager.Instance.caringZombieIndex = caringNum;
    }
}
