using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalTowerProjectile : Projectile
{
    [SerializeField]
    private float m_damageScale = 2f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnDealScaredDamageOnStunnedAndSlowedTarget.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_damageScale);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
