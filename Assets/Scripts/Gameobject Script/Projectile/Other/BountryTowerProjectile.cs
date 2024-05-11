using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BountryTowerProjectile : Projectile
{
    [SerializeField]
    private float m_changeToEarnGold = 0.07f;
    [SerializeField]
    private int m_goldToEarn = 77;

    protected override void OnHitTarget()
    {
        if (Random.Range(0, 100) < m_attackPower)
        {
            int id = TowerManager.Instance.m_towers[m_shootTowerID].GetComponent<Tower>().m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID();
            int newGold = PlayerStatsManager.Instance.GetPlayerGold(id) + m_goldToEarn;
            GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGold, id);
            print("GetMoney!!!");
        }
    }

    protected override void OnDestroyObject()
    {
        Destroy(this.gameObject);
    }
}
