using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuciferProjectile : Projectile
{
    private float m_executedScale = 0.16f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnExecuteEnemy.Trigger(m_enemyToShoot.GetEnemyID(), this.m_shootTowerID, m_executedScale);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
