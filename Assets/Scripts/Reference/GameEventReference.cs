using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventReference : Singleton<GameEventReference>
{
    public GameEvent OnEnemyHurt = new GameEvent();
    public GameEvent OnEnemyDestroyed = new GameEvent();

    public GameEvent OnTowerPlaced = new GameEvent();
    public GameEvent OnTowerChangeTarget = new GameEvent();

    public GameEvent OnStateChange = new GameEvent();
    public GameEvent OnPlayerModifyHealth = new GameEvent();
    public GameEvent OnPlayerModifyGold = new GameEvent();

    public GameEvent OnEnterPreparationState = new GameEvent();
    public GameEvent OnEnterBattleState = new GameEvent();
    public GameEvent OnEnterReposeState = new GameEvent();

    public GameEvent OnPlayerConsumeCard = new GameEvent();
}
