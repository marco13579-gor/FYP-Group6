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
    public GameObject m_roomCodeText;
    public GameObject m_startGameButton;
    public GameObject m_clickToCopyButton;
    public GameObject m_morePlayerJoinWarningText;
    public GameObject m_player1StatusText;
    public GameObject m_player2StatusText;
    public GameObject m_player3StatusText;
    public GameObject m_player4StatusText;
    public GameObject m_playerNumberText;

    public GameObject m_towerUpgradePanel;
    public GameObject m_towerUpgradeTowerAttackPowerButton;
    public GameObject m_towerUpgradeTowerAttackPowerText;
    public GameObject m_towerUpgradeTowerAttackSpeedButton;
    public GameObject m_towerUpgradeTowerAttackSpeedText;
    public GameObject m_towerUpgradeTowerAttackRangeButton;
    public GameObject m_towerUpgradeTowerAttackRangeText;
}
