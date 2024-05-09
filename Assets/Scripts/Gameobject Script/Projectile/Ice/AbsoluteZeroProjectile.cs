using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsoluteZeroProjectile : Projectile
{
    [SerializeField]
    private float slowScare = 0.99f;
    [SerializeField]
    private float slowDuration = 3f;
    [SerializeField]
    private float m_executedScale = 0.3f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnExecuteSlowedEnemy.Trigger(m_enemyToShoot.GetEnemyID(), this.m_shootTowerID, m_executedScale);
        GameEventReference.Instance.OnEnemySlowed.Trigger(m_enemyToShoot.GetEnemyID(), slowScare, slowDuration);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
