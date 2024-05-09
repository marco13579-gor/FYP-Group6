using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandNatureBullet : Projectile
{
    [SerializeField]
    private float stunDuration = 2f;
    [SerializeField]
    private float m_executedScale = 0.3f;

    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnExecuteStunnedEnemy.Trigger(m_enemyToShoot.GetEnemyID(), this.m_shootTowerID, m_executedScale);
        GameEventReference.Instance.OnEnemyStunned.Trigger(m_enemyToShoot.GetEnemyID(), stunDuration);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
