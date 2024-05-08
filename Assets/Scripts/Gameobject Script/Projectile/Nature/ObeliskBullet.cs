using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObeliskBullet : Projectile
{
    [SerializeField]
    private float m_rupturedRate = 0.2f;


    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnRuptureEnemy.Trigger(m_enemyToShoot.GetEnemyID(), m_rupturedRate);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}