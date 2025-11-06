using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HomeUIScript : MonoBehaviour
{
    // 좀비데이터
    public ZombieData target;

    public GameObject normalZombie;
    public GameObject sua;
    public GameObject quiz;
    public GameObject homePlayer;

    private int[] firstFloor = {5, 9, 5};
    private int[] secondFloor = {5, 15, 5};
    private int[] thirdFloor = {20, 16, 5};
    private int[] fourthFloor = {17, 10, 8, 5};
    private int[] fivethFloor = {0, 1, 2, 3, 4, 5};

    public List<int> remainCure = new List<int>();
    public int caringNum;

    public Image item;
    public CureEffectController cureEffect;
    private Tweener _fadeTween;
    
    // 좀비를 처음 데려온 경우 문제 초기화
    public void InitHomeUI()
    {
        DOTween.Kill(normalZombie);
        normalZombie.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        
        // 치료할게 남아있다면 초기화 하지 않음
        if (remainCure.Count > 0) return;
        
        // 1. 인벤토리 데이터를 리스트 변수에 저장합니다.
        var playerInventory = PlayerDataManager.Instance.PlayerInventoryData;
        
        // 2. 리스트를 끝에서부터 (역순으로) 순회합니다. (요소 제거 시 인덱스 오류 방지)
        for (int i = playerInventory.Count - 1; i >= 0; i--)
        {
            var invenData = playerInventory[i]; // 현재 인벤토리 데이터
        
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
                caringNum = invenData.index;
                sua.SetActive(true);

                foreach (var cure in fivethFloor)
                {
                    remainCure.Add(cure);
                }
            }
        }
        UpdateQuiz();
    }
    
    public void RemoveFirst()
    {
        if (remainCure.Count <= 0) return;

        remainCure.RemoveAt(0);
        UpdateQuiz();
        cureEffect.PlayEffects();
        
        SoundManager.Instance.PlaySFX(11);

        if (remainCure.Count <= 0)
        {
            homePlayer.GetComponent<Animator>().Play("PlayerBlood");
        }
    }

    private void UpdateQuiz()
    {
        if (remainCure.Count > 0)
        {
            quiz.SetActive(true);
            item.sprite = ItemDataManager.Instance.GetItemByIndex(remainCure[0]).itemSprite;
        }
        else
        {
            quiz.SetActive(false);
        }
    }

    public void ClearCaredZombie()
    {
        Debug.Log("ClearCaredZombie");
        SoundManager.Instance.PlaySFX(12);
        
        remainCure.Clear();
        caringNum = 0;
        target = null;

        if (normalZombie != null && normalZombie.activeSelf)
        {
            normalZombie.GetComponent<Animator>().Play("Normal1");
            FadeOutAndDisableImage(normalZombie, 1f, 1f);
        }

        if (sua != null && sua.activeSelf)
        {
            sua.GetComponent<Animator>().Play("Sua2");
            StartCoroutine(WaitAndOpenEnding(2f));
        }
    }
    
    private void FadeOutAndDisableImage(GameObject targetObject, float delay, float duration)
    {
        Image targetImage = targetObject.GetComponent<Image>();

        targetImage.color = Color.white;

        _fadeTween = targetImage.DOFade(0f, duration)
            .SetDelay(delay)
            .SetAutoKill(false)
            .OnComplete(() =>
            {
                targetObject.SetActive(false);
            });
    }
    
    private IEnumerator WaitAndOpenEnding(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetComponentInParent<ApartSceneController>().OpenEnding();
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.IsZombieInHome = remainCure.Count > 0;
        _fadeTween?.Kill();
        if (remainCure.Count <= 0)
            normalZombie.SetActive(false);
    }
}