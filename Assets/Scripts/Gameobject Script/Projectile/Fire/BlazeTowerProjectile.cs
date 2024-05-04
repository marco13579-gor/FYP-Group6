using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlazeTowerProjectile : Projectile
{
    [SerializeField]
    private float m_scare = 0.5f;

    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnSlowedTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_scare);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
