using System.Collections.Generic;
using UnityEngine;

public class UI_Hp : MonoBehaviour
{
    [SerializeField] private List<Animator> hearts;
    private int index = 3;

    private void DecreaseHp()
    {
        if (--index < 0) 
            return;
        hearts[index].Play("Heartbreak");
    }
}
