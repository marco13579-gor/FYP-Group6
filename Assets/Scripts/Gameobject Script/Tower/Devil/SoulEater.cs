using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulEater : BaseTower
{
    [SerializeField]
    private int healthToReduce = 5;
    protected override void ReposeAction()
    {
        int newHealth = PlayerStatsManager.Instance.GetPlayerHealth(m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID()) - healthToReduce;
        GameEventReference.Instance.OnPlayerModifyHealth.Trigger(newHealth, m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID());
    }
}
