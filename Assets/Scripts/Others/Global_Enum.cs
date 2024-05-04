using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global_Enum : MonoBehaviour
{
}

public enum TowerType
{
    Tower1x1,
    Tower2x2,
    Tower1x2,
    BlazeTower,
    HotCoalsTower,
    Inferno,
    FreezeTower,
    IceTower,
    CrystalTower,
    TempestTower,
    UnstableTower,
    Disaster,
    Pertubable,
    HardSteelTower,
    EvilTower,
    SatanTower,
    TheDevilDeal,
    Lucifer,
    GoldMine,
    BountryTower,
    RedCrimson
}

public enum GameState 
{
    Preparation,
    Battle,
    Repose
}

public enum UpgradeCore
{
    AttackPower,
    AttackRange,
    AttackSpeed
}

public enum TowerTargetsSelectCondition
{
    MaxHealth,
    MaxSpeed,
    MinHealth,
    MinSpeed,
    FirstTarget,
    LastTarget
}

public enum CardDrawState
{
    EarlyGame,
    MidGame,
    LateGame
}