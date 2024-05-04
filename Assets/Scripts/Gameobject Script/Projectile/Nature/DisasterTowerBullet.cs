using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisasterTowerBullet : Projectile
{
    [SerializeField]
    private float m_damageScale = 0.3f;
    [SerializeField]
    private int m_burnCount = 4;


    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnIgniteStunnedEnemy.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower, m_shootTowerID, m_attackPower * m_damageScale, m_burnCount);
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
