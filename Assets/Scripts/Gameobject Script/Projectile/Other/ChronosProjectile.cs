using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChronosProjectile : Projectile
{
    [SerializeField]
    private float slowScare = 0.8f;
    [SerializeField]
    private float slowDuration = 10f;
    [SerializeField]
    private float m_ruptureScale = 0.8f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnEnemySlowed.Trigger(m_enemyToShoot.GetEnemyID(), slowScare, slowDuration);
        GameEventReference.Instance.OnRuptureEnemy.Trigger(m_enemyToShoot.GetEnemyID(), m_ruptureScale);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
