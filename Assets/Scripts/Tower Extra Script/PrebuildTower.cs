using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrebuildTower : MonoBehaviour
{
    [SerializeField]
    private Tower m_toBuildTower;
    public TowerSO m_towerSO;
    public Tower GetTower() { return m_toBuildTower;}
}
