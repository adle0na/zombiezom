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
    [InspectorName("작은피묻은박스")]
    BloodBox_S,
    [InspectorName("작은구겨진박스")]
    CrumpledBox_S,
    [InspectorName("작은열린박스")]
    OpendBox_S,
    [InspectorName("큰피묻은박스")]
    BloodBox_L,
    [InspectorName("큰구겨진박스")]
    CrumpledBox_L,
    [InspectorName("큰열린박스")]
    OpendBox_L
}