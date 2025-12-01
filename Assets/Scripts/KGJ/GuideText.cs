using System;
using TMPro;
using UnityEngine;

public class GuideText : MonoBehaviour
{
    private UI_Inventory _inventory;
    private TMP_Text _text;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        _inventory = FindAnyObjectByType<UI_Inventory>();
        _inventory.OnEnableInventory += UpdateText;
    }

    private void OnDestroy()
    {
        _inventory.OnEnableInventory -= UpdateText;
    }

    private void UpdateText(bool isEnable)
    {
        if (!isEnable)
            _text.text = "A/D : 좌우 이동\nSpacebar : 상호작용\nESC : 환경설정";
        else
            _text.text = "좌클릭 : 정보 보기\n우클릭 : 아이템 버리기";
    }
}
