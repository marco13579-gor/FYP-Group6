using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TowerTargeting : NetworkBehaviour
{
    [SerializeField] private LayerMask m_enemyMask;

    private Tower m_tower;
    private Enemy m_targetEnemy;

    private List<GameObject> m_availableTargets = new List<GameObject>();

    private void Awake()
    {
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnTowerPlaced.AddListener(OnTowerPlaced);
        GameEventReference.Instance.OnEnemyDestroyed.AddListener(OnEnemyDestroyed);
    }

    private void Update()
    {
        if (!m_tower || !m_tower.m_isPlaced)
        {
            return;
        }

        if (m_availableTargets.Count == 0)
        {
            m_targetEnemy = null;
        }
        else
        {
            if (m_targetEnemy == null || !m_availableTargets[0].GetComponent<Enemy>().Equals(m_targetEnemy))
            {
                m_targetEnemy = m_availableTargets[0].GetComponent<Enemy>();
                GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
            }
        }
    }

    private void OnTowerPlaced(params object[] param)
    {
        int towerID = (int)param[0];
        TowerSO towerSO = (TowerSO)param[1];
        GameObject originTile = (GameObject)param[2];

        Tower placedTower = TowerManager.Instance.GetTower(towerID).GetComponent<Tower>();

        if (placedTower == this.m_tower)
        {
            this.m_tower = placedTower;
        }
    }

    private void OnEnemyDestroyed(params object[] param)
    {
        GameObject enemy = (GameObject)param[0];

        if (m_availableTargets.Contains(enemy))
        {
            m_availableTargets.Remove(enemy);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject target = other.transform.gameObject;

        if (target.tag == "Enemy" && !m_availableTargets.Contains(target))
        {
            m_availableTargets.Add(target);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject target = other.transform.gameObject;

        if (target.tag == "Enemy" && m_availableTargets.Contains(target))
        {
            m_availableTargets.Remove(target);
        }
    }

    private bool CheckTargetIsInRange()
    {
        return Vector3.Distance(m_targetEnemy.transform.position, transform.position) <= m_tower.m_towerSO.m_attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_tower == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_tower.m_towerSO.m_attackRange);
    }

    public void SetTower(Tower tower) => m_tower = tower;
}
