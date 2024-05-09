using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HellFireProject : Projectile
{
    [SerializeField]
    private float slowScare = 0.7f;
    [SerializeField]
    private float slowDuration = 5f;
    [SerializeField]
    private float m_executedScale = 0.3f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnExecuteIgnitedEnemy.Trigger(m_enemyToShoot.GetEnemyID(), this.m_shootTowerID, m_executedScale);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
