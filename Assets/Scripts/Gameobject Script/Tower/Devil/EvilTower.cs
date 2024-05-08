using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvilTower : BaseTower
{
    [SerializeField]
    private int healthToReduce = 1;
    protected override void ReposeAction()
    {
        int newHealth = PlayerStatsManager.Instance.GetPlayerHealth(m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID()) - healthToReduce;
        GameEventReference.Instance.OnPlayerModifyHealth.Trigger(newHealth, m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID());
    }
}
