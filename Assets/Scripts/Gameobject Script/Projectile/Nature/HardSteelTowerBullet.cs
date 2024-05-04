using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardSteelTowerBullet : Projectile
{
    [SerializeField]
    private float stunDuration = 3f;

    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyStunned.Trigger(m_enemyToShoot.GetEnemyID(), stunDuration);
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
