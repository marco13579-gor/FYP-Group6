using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTowerProejectile : Projectile
{
    [SerializeField]
    private float slowScare = 0.7f;
    [SerializeField]
    private float slowDuration = 5f;
    protected override void OnHitTarget()
    {
        GameEventReference.Instance.OnEnemyHurt.Trigger(m_enemyToShoot.GetEnemyID(), m_attackPower);
        GameEventReference.Instance.OnEnemySlowed.Trigger(m_enemyToShoot.GetEnemyID(), slowScare, slowDuration);
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
