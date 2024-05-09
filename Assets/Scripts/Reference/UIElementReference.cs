using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementReference : Singleton<UIElementReference>
{
    public GameObject m_stateText;
    public GameObject m_healthText;
    public GameObject m_goldText;
    public GameObject m_timerText;
    public GameObject m_mobsremainingText;

    public GameObject m_relayPanel;
    public GameObject m_createRoomButton;
    public GameObject m_JoinRoomButton;
    public GameObject m_InputRoomCodeField;
    public GameObject m_warningText;

    public GameObject m_roomPanel;

    public GameObject m_towerUpgradePanel;
    public GameObject m_towerUpgradeTowerAttackPowerButton;
    public GameObject m_towerUpgradeTowerAttackPowerText;
    public GameObject m_towerUpgradeTowerAttackSpeedButton;
    public GameObject m_towerUpgradeTowerAttackSpeedText;
    public GameObject m_towerUpgradeTowerAttackRangeButton;
    public GameObject m_towerUpgradeTowerAttackRangeText;
    public GameObject m_towerTargetConditionFirstTargetButton;
    public GameObject m_towerTargetConditionLastTargetButton;
    public GameObject m_towerTargetConditionMaxHealthButton;
    public GameObject m_towerTargetConditionMinHealthButton;
    public GameObject m_towerTargetConditionMaxSpeedButton;
    public GameObject m_towerTargetConditionMinSpeedButton;
    public GameObject m_removeTowerButton;
    public GameObject m_towerImage;
    public GameObject m_attackPowerText;
    public GameObject m_attackRangeText;
    public GameObject m_attacSpeedText;
    public GameObject m_currentAtkMode;
    public GameObject m_desriptionText;

    public GameObject m_auctionPanel;
    public GameObject m_auctionCardSlot;
    public GameObject m_bidButton;
    public GameObject m_giveupButton;
    public GameObject m_currentBidderText;
    public GameObject m_bidAmountText;
    public GameObject m_bidAmountInputField;
    public GameObject m_player1GoldText;
    public GameObject m_player2GoldText;
    public GameObject m_player3GoldText;
    public GameObject m_player4GoldText;

    public GameObject m_EndGamePanel;
    public GameObject m_victoryGame;
    public GameObject m_loseGame;

    public GameObject m_player1StatusObject;
    public GameObject m_player2StatusObject;
    public GameObject m_player3StatusObject;
    public GameObject m_player4StatusObject;
    public GameObject m_player1HealthText;
    public GameObject m_player2HealthText;
    public GameObject m_player3HealthText;
    public GameObject m_player4HealthText;
    public GameObject m_player1StatusGoldText;
    public GameObject m_player2StatusGoldText;
    public GameObject m_player3StatusGoldText;
    public GameObject m_player4StatusGoldText;

    public GameObject m_restartGameButton;
    public GameObject m_turnText;

    public GameObject m_loseGamePanel;
    public GameObject m_restartButton;
    public GameObject m_loseGameButton;

    public GameObject m_player1Sheild;
    public GameObject m_player2Sheild;
    public GameObject m_player3Sheild;
    public GameObject m_player4Sheild;

    public GameObject m_resetButtonText;

    public GameObject m_bloodScreen;

    public GameObject m_loseScreen;
}
