using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheDevilDealProjectile : Projectile
{
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
