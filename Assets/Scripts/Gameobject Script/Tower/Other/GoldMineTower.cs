using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMineTower : Tower
{
    protected override void ReposeAction()
    {
        GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID()) + (int)GetAttackPower() * GameStateManager.Instance.GetGameTurn(), m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID());
    }
}
