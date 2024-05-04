using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PertubableBullet : Projectile
{
    [SerializeField]
    private float m_damageScale = 0.3f;
    [SerializeField]
    private float m_extraDamageScale = 1.5f;


    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnStunnedAndSlowedTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_extraDamageScale);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
