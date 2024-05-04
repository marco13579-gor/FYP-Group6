using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempestTowerProjectile : Projectile
{
    [SerializeField]
    private float m_damageScale = 1.5f;
    [SerializeField]
    private float m_slowScale = 0.4f;
    [SerializeField]
    private float m_slowDuration = 6f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnSlowedTargetOrSlowTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_damageScale, m_slowScale, m_slowDuration);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
