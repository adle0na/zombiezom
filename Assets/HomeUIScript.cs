using System.Collections.Generic;
using UnityEngine;

public class HomeUIScript : MonoBehaviour
{
    // 좀비데이터
    public ZombieData target;

    public GameObject normalZombie;
    public GameObject sua;
    public GameObject chair;

    private int[] firstFloor = {5, 9};
    private int[] secondFloor = {5, 15};
    private int[] thirdFloor = {20, 16, 5};
    private int[] fourthFloor = {17, 10, 8, 5};
    private int[] fivethFloor = {0, 1, 2, 3, 4, 5};

    public List<int> remainCure = new List<int>();
    public int caringNum;
    
    public void InitHomeUI()
    {
                    foreach (var invenData in PlayerDataManager.Instance.PlayerInventoryData)
                    {
                        if (invenData.index > 21 && invenData.index < 26)
                        {
                            normalZombie.SetActive(true);
            
                            caringNum = invenData.index;
                            
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
                            sua.SetActive(true);
                            
                            foreach (var cure in fivethFloor)
                            {
                                remainCure.Add(cure);
                            }
                        }
                    }
                    PlayerDataManager.Instance.IsZombieInHome = remainCure.Count > 0;
        
    }

    public void QuitHome()
    {
        if (remainCure.Count > 0)
        {
            PlayerDataManager.Instance.IsZombieInHome = true;
            PlayerDataManager.Instance.homeUIPrefab = gameObject;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(this);   
        }
        
        PlayerDataManager.Instance.playerObj.SetActive(true);
    }
}
