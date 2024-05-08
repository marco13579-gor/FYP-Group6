using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCrimsonProjectile : Projectile
{
    [SerializeField]
    private float m_scare = 1.75f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnIgnitedAmountTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_scare);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
