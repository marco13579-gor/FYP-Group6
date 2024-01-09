using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrebuildTower : MonoBehaviour
{
    [SerializeField]
    private Tower m_toBuildTower;
    [SerializeField]
    private GameObject m_rangeIndiactor;
    public TowerSO m_towerSO;

    private void Start()
    {
        m_rangeIndiactor.transform.localScale = new Vector3(m_towerSO.m_attackRange, 1f, m_towerSO.m_attackRange);
    }

    public Tower GetTower() { return m_toBuildTower;}
}
