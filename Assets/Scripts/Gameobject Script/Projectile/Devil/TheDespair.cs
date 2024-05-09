using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheDespair : Projectile
{
    [SerializeField]
    private float m_ruptureScale = 0.3f;

    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnRuptureEnemy.Trigger(m_enemyToShoot.GetEnemyID(), m_ruptureScale);
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
