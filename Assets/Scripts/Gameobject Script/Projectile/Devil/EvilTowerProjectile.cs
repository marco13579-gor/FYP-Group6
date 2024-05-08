using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilTowerProjectile : Projectile
{
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        //GameEventReference.Instance.OnExecuteEnemy.Trigger(m_enemyToShoot.GetEnemyID(), m_shootTowerID, 1f);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
