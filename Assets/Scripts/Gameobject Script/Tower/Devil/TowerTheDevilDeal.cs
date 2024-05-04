using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTheDevilDeal : BaseTower
{
    [SerializeField]
    private int goldToReduce = 6;
    [SerializeField]
    private int healthToIncrease = 1;
    protected override void ReposeAction()
    {
        if (PlayerStatsManager.Instance.GetPlayerGold(m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID()) > 6)
        {
            int newGold = PlayerStatsManager.Instance.GetPlayerGold(m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID()) - goldToReduce;
            GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGold, m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID());
            int newHealth = PlayerStatsManager.Instance.GetPlayerGold(m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID()) + healthToIncrease;
            GameEventReference.Instance.OnPlayerModifyHealth.Trigger(newHealth, m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID());
        }
    }
}
