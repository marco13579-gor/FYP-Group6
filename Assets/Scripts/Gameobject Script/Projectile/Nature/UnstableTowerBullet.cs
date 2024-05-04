using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnstableTowerBullet : Projectile
{
    [SerializeField]
    private float m_damageScale = 2f;
    [SerializeField]
    private float m_slowScale = 0.2f;
    [SerializeField]
    private float m_slowDuration = 4f;

    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageAndSlowOnStunnedEnemy.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_damageScale, m_slowScale, m_slowDuration);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
