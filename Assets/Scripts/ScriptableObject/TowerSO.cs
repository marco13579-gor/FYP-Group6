using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "new Tower", menuName = "Battle System/Tower Scriptable Object", order = 3)]
public class TowerSO : ScriptableObject
{
    [Header("Tower Setting")]
    public string m_name = "default Tower";
    public TowerType m_towerType;
    public int m_attackPower;
    public int m_attackSpeed;
    public float m_attackRange;
    public int m_cost;
    public Projectile m_attackObject;
    public int[] m_tileToBuild;
    public Vector3 m_offset;
}