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
    public float m_attackPower;
    public float m_attackSpeed;
    public float m_attackRange;
    public bool m_isAreaAttack;
    public int m_cost;
    public int[] m_tileToBuild;
    public Vector3 m_offset;

    public Sprite m_sprite;
    public string m_desritption;
}