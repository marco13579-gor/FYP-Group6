using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TowerTargeting : NetworkBehaviour
{
    private Tower m_tower;
    private Enemy m_targetEnemy;

    public List<GameObject> m_availableTargets = new List<GameObject>();

    private TowerTargetsSelectCondition m_towerTargetSelectCondition = TowerTargetsSelectCondition.FirstTarget;
    private void Awake()
    {
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnTowerPlaced.AddListener(OnTowerPlaced);
        GameEventReference.Instance.OnEnemyDestroyed.AddListener(OnEnemyDestroyed);

        GameEventReference.Instance.OnEnterReposeState.AddListener(OnEnterReposeState);
    }

    private void Update()
    {
        if (!m_tower || !m_tower.m_isPlaced)
        {
            return;
        }

        if (!IsServer)
            return;

        m_availableTargets.RemoveAll(currentObj => currentObj == null);

        CheckEnemyRangeToRemoveLogic();

        if (m_availableTargets.Count == 0)
        {
            m_targetEnemy = null;
            m_tower.RemoveTarget();
        }
        else
        {
            if (m_tower.GetTargetEnemy() == null && m_availableTargets[0] != null)
            {
                GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_availableTargets[0].GetComponent<Enemy>());
            }

            switch (m_towerTargetSelectCondition)
            {
                case TowerTargetsSelectCondition.FirstTarget:
                    if (m_targetEnemy == null && m_availableTargets != null && m_availableTargets[0] != null && !m_availableTargets[0].GetComponent<Enemy>().Equals(m_targetEnemy))
                    {
                        if (m_availableTargets[0].GetComponent<Enemy>() != null && !m_availableTargets[0].GetComponent<Enemy>().GetDieStatus())
                        {
                            m_targetEnemy = m_availableTargets[0].GetComponent<Enemy>();
                            GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
                            m_tower.SetTargetEnemyList(m_availableTargets);
                        }
                    }
                    else
                    {
                        m_targetEnemy = null;
                        m_tower.RemoveTarget();
                    }
                    break;
                case TowerTargetsSelectCondition.LastTarget:
                    if (m_targetEnemy == null && m_availableTargets != null && m_availableTargets[0] != null && !m_availableTargets[m_availableTargets.Count - 1].GetComponent<Enemy>().Equals(m_targetEnemy))
                    {
                        if (m_availableTargets[0].GetComponent<Enemy>() != null)
                        {
                            m_targetEnemy = m_availableTargets[m_availableTargets.Count - 1].GetComponent<Enemy>();
                            GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
                            m_tower.SetTargetEnemyList(m_availableTargets);
                        }
                    }
                    else
                    {
                        m_targetEnemy = null;
                        m_tower.RemoveTarget();
                    }
                    break;
                case TowerTargetsSelectCondition.MaxHealth:
                    float maxHealthPoint = 0;
                    int maxHealthPointEnemyIndex = 0;
                    for (int i = 0; i < m_availableTargets.Count; i++)
                    {
                        if (m_availableTargets == null) return;
                        if (m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth() > maxHealthPoint)
                        {
                            maxHealthPoint = m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth();
                            maxHealthPointEnemyIndex = i;
                        }
                    }
                    if (m_targetEnemy == null && m_availableTargets != null && m_availableTargets[0] != null && !m_availableTargets[maxHealthPointEnemyIndex].GetComponent<Enemy>().Equals(m_targetEnemy))
                    {
                        if (m_availableTargets[maxHealthPointEnemyIndex].GetComponent<Enemy>() != null)
                        {
                            m_targetEnemy = m_availableTargets[maxHealthPointEnemyIndex].GetComponent<Enemy>();
                            GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
                            m_tower.SetTargetEnemyList(m_availableTargets);
                        }
                    }
                    else
                    {
                        m_targetEnemy = null;
                        m_tower.RemoveTarget();
                    }
                    break;
                case TowerTargetsSelectCondition.MinSpeed:
                    float minSpeedPoint = 999999999;
                    int minSpeedPointEnemyIndex = 0;
                    for (int i = 0; i < m_availableTargets.Count; i++)
                    {
                        if (m_availableTargets[i].GetComponent<Enemy>() == null) return;
                        if (m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth() < minSpeedPoint)
                        {
                            minSpeedPoint = m_availableTargets[i].GetComponent<Enemy>().GetEnemySpeed();
                            minSpeedPointEnemyIndex = i;
                        }
                    }
                    if (m_targetEnemy == null && m_availableTargets != null && m_availableTargets[0] != null && !m_availableTargets[minSpeedPointEnemyIndex].GetComponent<Enemy>().Equals(m_targetEnemy))
                    {
                        if (m_availableTargets[minSpeedPointEnemyIndex].GetComponent<Enemy>() != null)
                        {
                            m_targetEnemy = m_availableTargets[minSpeedPointEnemyIndex].GetComponent<Enemy>();
                            GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
                            m_tower.SetTargetEnemyList(m_availableTargets);
                        }
                    }
                    else
                    {
                        m_targetEnemy = null;
                        m_tower.RemoveTarget();
                    }
                    break;
                case TowerTargetsSelectCondition.MaxSpeed:
                    float maxSpeedPoint = 0;
                    int maxSpeedPointEnemyIndex = 0;
                    for (int i = 0; i < m_availableTargets.Count; i++)
                    {
                        if (m_availableTargets[i].GetComponent<Enemy>() == null) return;
                        if (m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth() > maxSpeedPoint)
                        {
                            maxSpeedPoint = m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth();
                            maxSpeedPointEnemyIndex = i;
                        }
                    }
                    if (m_targetEnemy == null && m_availableTargets != null && m_availableTargets[0] != null && !m_availableTargets[maxSpeedPointEnemyIndex].GetComponent<Enemy>().Equals(m_targetEnemy))
                    {
                        if (m_availableTargets[maxSpeedPointEnemyIndex].GetComponent<Enemy>() != null)
                        {
                            m_targetEnemy = m_availableTargets[maxSpeedPointEnemyIndex].GetComponent<Enemy>();
                            GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
                            m_tower.SetTargetEnemyList(m_availableTargets);
                        }
                    }
                    else
                    {
                        m_targetEnemy = null;
                        m_tower.RemoveTarget();
                    }
                    break;
                case TowerTargetsSelectCondition.MinHealth:
                    float minHealthPoint = 999999999;
                    int minHealthPointEnemyIndex = 0;
                    for (int i = 0; i < m_availableTargets.Count; i++)
                    {
                        if (m_availableTargets[i].GetComponent<Enemy>() == null) return;
                        if (m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth() < minHealthPoint)
                        {
                            minHealthPoint = m_availableTargets[i].GetComponent<Enemy>().GetEnemyHealth();
                            minHealthPointEnemyIndex = i;
                        }
                    }
                    if (m_targetEnemy == null && m_availableTargets != null && m_availableTargets[0] != null && !m_availableTargets[minHealthPointEnemyIndex].GetComponent<Enemy>().Equals(m_targetEnemy))
                    {
                        if (m_availableTargets[minHealthPointEnemyIndex] != null)
                        {
                            m_targetEnemy = m_availableTargets[minHealthPointEnemyIndex].GetComponent<Enemy>();
                            GameEventReference.Instance.OnTowerChangeTarget.Trigger(m_tower.GetTowerID(), m_targetEnemy);
                            m_tower.SetTargetEnemyList(m_availableTargets);
                        }
                    }
                    else
                    {
                        m_targetEnemy = null;
                        m_tower.RemoveTarget();
                    }
                    break;
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
            m_tower.RemoveTarget();
            m_tower.SetTargetEnemyList(m_availableTargets);
        }
    }

    private void OnEnterReposeState(params object[] param)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //GameObject target = other.transform.gameObject;

        //if (target.tag == "Enemy" && !m_availableTargets.Contains(target))
        //{
        //    m_availableTargets.Add(target);
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //GameObject target = other.transform.gameObject;

        //if (target.tag == "Enemy" && m_availableTargets.Contains(target))
        //{
        //    m_availableTargets.Remove(target);

        //    if (target == m_targetEnemy)
        //    {
        //        m_targetEnemy = null;
        //        m_tower.RemoveTarget();
        //    }
        //}
    }

    private void CheckEnemyRangeToRemoveLogic()
    {
        foreach (GameObject target in EnemyManager.Instance.m_SpawnedEnemies.Values.ToList())
        {
            float distance = Vector3.Distance(target.transform.position, m_tower.transform.position);
            if (distance > m_tower.GetAttackRange())
            {
                if (m_availableTargets.Contains(target))
                    m_availableTargets.Remove(target);
            }
            else
            {
                if (!m_availableTargets.Contains(target))
                {
                    m_availableTargets.Add(target);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (m_tower == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_tower.GetAttackRange());
    }

    public void SetTower(Tower tower) => m_tower = tower;
    public void SetTowerTargetCondition(TowerTargetsSelectCondition towerTargetsSelectCondition) => m_towerTargetSelectCondition = towerTargetsSelectCondition;

    public string TargetConditionToString() => m_towerTargetSelectCondition.ToString();
}
