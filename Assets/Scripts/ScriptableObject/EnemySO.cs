using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new Enemy", menuName = "Battle System/Enemy Scriptable Object", order = 1)]
public class EnemySO : ScriptableObject
{
    [Header("Enemy Setting")]
    public string m_name = "default Enemy";
    public float m_maxHealth = 10;
    public float m_health;
    public int m_attackPower = 5;
    public float m_movementSpeed = 5;
    public int m_rewardGold = 1;
}