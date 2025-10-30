using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_Hp : MonoBehaviour
{
    [SerializeField] private List<Animator> hearts;
    private int index = 3;
    
    private void Start()
    {
        PlayerDataManager.Instance.OnHpDecreaseEvent += DecreaseHp;
        PlayerDataManager.Instance.OnGetFullHpEvent += FullHp;
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance == null)
            return;
        
        PlayerDataManager.Instance.OnHpDecreaseEvent -= DecreaseHp;
        PlayerDataManager.Instance.OnGetFullHpEvent -= FullHp;
    }

    private void DecreaseHp()
    {
        if (--index < 0) 
            return;
        hearts[index].Play("Heartbreak");
    }

    private void FullHp()
    {
        foreach (var heart in hearts)
        {
            heart.Play("HeartRed");
        }
    }
}
