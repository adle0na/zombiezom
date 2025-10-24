using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class FloorData
{
    [LabelText("층 수"), ReadOnly]
    public int floorValue;
    
    [LabelText("층 타입"), ReadOnly]
    public FloorType floorType;

    [LabelText("닫힌문 생성 배수 (0~1)")]
    [Range(0f, 1f)]
    public float closedPer;
}

[Serializable]
public class DoorData
{
    [LabelText("문 디자인 타입")]
    public DoorType doorType;
    [LabelText("문 열리는 여부")]
    public bool isOpenable;
    [LabelText("박스 존재 여부")]
    public bool hasBox;
    [LabelText("박스 데이터")]
    public BoxData boxData;
}

[Serializable]
public class BoxData
{
    [LabelText("박스 타입")]
    public BoxType boxType;
    [LabelText("박스 아이템 리스트")]
    public List<ItemData> boxItems;
    [LabelText("사용 여부")]
    public bool isOpened;
}

[Serializable]
public class ItemData
{
    public int itemIndex;
    public Sprite itemSprite;
    public string itemName;
    public string itemDes;
}