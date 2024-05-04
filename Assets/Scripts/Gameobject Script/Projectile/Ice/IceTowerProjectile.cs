using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTowerProjectile : Projectile
{
    [SerializeField]
    private float m_damageScale = 1.5f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnSlowedTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_damageScale);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
