using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotCoalTowerProjectile : Projectile
{
    [SerializeField]
    int triggerCount = 7;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyIgnited.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower / 5, this.m_shootTowerID, triggerCount);
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
