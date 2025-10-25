using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class Zombie : MonoBehaviour
{
    [LabelText("애니메이터")]
    [SerializeField] private Animator animator;

    [LabelText("좀비 데이터")]
    [SerializeField] private ZombieData zombieData;
}
