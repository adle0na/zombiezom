using System;
using System.Collections;
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

    [LabelText("닫힌문 생성 배수")]
    [Range(0f, 1f)]
    public float closedPer;

    [LabelText("생성 아이템 리스트")]
    public List<int> itemList;

    [LabelText("좀비 리스트")]
    public List<ZombieData> zombieDatas;
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
    [LabelText("왼쪽 위치")]
    public Transform leftBoxPos;
    [LabelText("오른쪽 위치")]
    public Transform rightBoxPos;
}

[Serializable]
public class BoxData
{
    [LabelText("박스 타입")]
    public BoxType boxType;
    [LabelText("박스 위치")]
    public bool isLeft;
    [LabelText("박스 아이템 리스트")]
    public List<ItemCsvRow> boxItems;
    [LabelText("사용 여부")]
    public bool isOpened;
}

[Serializable]
public class ZombieData
{
    [LabelText("생성 위치")]
    public bool isLeftSpawn;

    [LabelText("좀비 타입")]
    public ZombieType zombieType;

    [LabelText("치료 아이템")]
    public List<int> cureItem;
}

[Serializable]
public class ItemCsvRow
{
    public int index;
    public string itemName;
    public Sprite itemSprite; // CSV의 ItemSprite 문자열을 Resources 경로로 사용
    public string itemDes;
    public int[] cureFloor;
    public int[] appearFloor;
}