using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatanTower : Projectile
{
    [SerializeField]
    private float m_slowScale = 0.6f;
    [SerializeField]
    private float m_slowDuration = 6;

    [SerializeField]
    private float m_igniteDamageScale = 0.6f;
    [SerializeField]
    private int m_ignitedCount = 6;

    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemySlowed.Trigger(m_enemyToShoot.GetEnemyID(), m_slowScale, m_slowDuration);
        GameEventReference.Instance.OnEnemyIgnited.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower * m_igniteDamageScale, m_shootTowerID, m_ignitedCount);
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
