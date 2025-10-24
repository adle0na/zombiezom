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
    [InspectorName("일반문")]
    NormalDoor,
    [InspectorName("피묻은문")]
    BloodDoor,
    [InspectorName("판자문")]
    ClosedDoor
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
    BloodBox_L
}