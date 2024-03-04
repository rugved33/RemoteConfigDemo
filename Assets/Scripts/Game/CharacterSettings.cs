using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Settings", menuName = "CombatSetting/Character Settings", order = 51)]
public class CharacterSettings : ScriptableObject
{
    public float MovementSpeed;
    public float AttackRange;
    public float RotateSpeed;
    public int MaxHp;
    public float ChaseRange;
    public float AttackRate;
}

