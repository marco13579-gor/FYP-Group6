using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventReference : Singleton<GameEventReference>
{
    public GameEvent OnEnemyHurt = new GameEvent();

    public GameEvent OnEnemyIgnited = new GameEvent();
    public GameEvent OnDealScaredDamageOnIgnitedTarget = new GameEvent();
    public GameEvent OnDealScaredDamageOnIgnitedAmountTarget = new GameEvent();
    public GameEvent OnDealScaredDamageOnSlowedTarget = new GameEvent();
    public GameEvent OnDealScaredDamageOnStunnedTarget = new GameEvent();

    public GameEvent OnDealScaredDamageOnStunnedAndSlowedTarget = new GameEvent();
    public GameEvent OnDealScaredDamageOnSlowedTargetOrSlowTarget = new GameEvent();
    public GameEvent OnIgniteStunnedTarget = new GameEvent();
    public GameEvent OnDealScaredDamageAndSlowTargetOnStunnedTarget = new GameEvent();
    public GameEvent OnDealScaredDamageOnIgnitedAndStunnedTarget = new GameEvent();
    public GameEvent OnDealScaredDamageAndSlowOnStunnedEnemy = new GameEvent();
    public GameEvent OnIgniteStunnedEnemy = new GameEvent();

    public GameEvent OnExecuteIgnitedEnemy = new GameEvent();
    public GameEvent OnExecuteSlowedEnemy = new GameEvent();
    public GameEvent OnExecuteStunnedEnemy = new GameEvent();
    public GameEvent OnExecuteEnemy = new GameEvent();

    public GameEvent OnEnemySlowed = new GameEvent();
    public GameEvent OnEnemyStunned = new GameEvent();

    public GameEvent OnEnemyDestroyed = new GameEvent();

    public GameEvent OnTowerPlaced = new GameEvent();
    public GameEvent OnTowerRemoved = new GameEvent();
    public GameEvent OnTowerChangeTarget = new GameEvent();

    public GameEvent OnStateChange = new GameEvent();
    public GameEvent OnPlayerModifyHealth = new GameEvent();
    public GameEvent OnPlayerModifyGold = new GameEvent();

    public GameEvent OnEnterPreparationState = new GameEvent();
    public GameEvent OnEnterBattleState = new GameEvent();
    public GameEvent OnEnterReposeState = new GameEvent();

    public GameEvent OnEnterAuctionState = new GameEvent();

    public GameEvent OnPlayerConsumeCard = new GameEvent();

    public GameEvent OnAuctionEnd = new GameEvent();
}
