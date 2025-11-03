using System;
using UnityEngine;

public enum FloorType
{
    [InspectorName("맨 위층")]
    Top,
    [InspectorName("중앙층")]
    Middle,
    [InspectorName("맨 아래층")]
    Bottom
}

public enum DoorType
{
    [InspectorName("판자문1")]
    ClosedDoorA,
    [InspectorName("판자문2")]
    ClosedDoorB,
    [InspectorName("일반문1")]
    NormalDoorA,
    [InspectorName("일반문2")]
    NormalDoorB,
    [InspectorName("피묻은문1")]
    BloodDoorA,
    [InspectorName("피묻은문2")]
    BloodDoorB,
    [InspectorName("피묻은문3")]
    BloodDoorC
}

public enum BoxType
{
    [InspectorName("작은 일반 박스")]
    NormalBox_S,
    [InspectorName("작은 더러운 박스")]
    DirtyBox_S,
    [InspectorName("작은 찌그러진 박스")]
    CrumpledBox_S,
    [InspectorName("작은 피묻은 박스")]
    BloodBox_S,
    [InspectorName("큰 일반 박스")]
    NormalBox_L,
    [InspectorName("큰 찌그러진 박스")]
    CrumpledBox_L,
    [InspectorName("큰 피묻은 박스")]
    BloodBox_L,
    [InspectorName("애용이 박스")]
    CatBox_S
}

public enum ZombieType
{
    [InspectorName("일반좀비")]
    NormalZombie,
    [InspectorName("치료불가좀비")]
    DisCureZombie,
    [InspectorName("수아좀비")]
    SuaZombie
}

public enum ZombieStatus
{
    [InspectorName("대기")]
    Idle,
    [InspectorName("발견")]
    PlayerFound,
    [InspectorName("스턴")]
    Stuned
}