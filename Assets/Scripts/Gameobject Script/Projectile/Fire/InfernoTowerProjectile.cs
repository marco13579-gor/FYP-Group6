using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfernoTowerProjectile : Projectile
{
    [SerializeField]
    private float m_scare = 1.5f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnIgnitedTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_scare);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
