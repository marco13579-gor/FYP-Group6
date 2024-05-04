using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    private bool m_papartionTimerCountDownTriggered = true;
    private bool m_reposeTimerCountDownTriggered = true;

    private float m_preparationTimer;
    private float m_reposeTimer;

    private float m_timerText;

    private NetworkVariable<FixedString4096Bytes> m_stateText = new NetworkVariable<FixedString4096Bytes>("Unknown", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        SetUpListeners();
        m_stateText.OnValueChanged += UpdateStateText;
    }

    private void Update()
    {
        UIElementReference.Instance.m_turnText.GetComponent<TMP_Text>().text = GameStateManager.Instance.GetGameTurn().ToString();
        if (NetworkManager.Singleton.IsClient && !m_papartionTimerCountDownTriggered)
        {
            if (Time.time >= m_preparationTimer)
            {
                UIElementReference.Instance.m_player1GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[0].ToString();
                UIElementReference.Instance.m_player2GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[1].ToString();
                UIElementReference.Instance.m_player3GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[2].ToString();
                UIElementReference.Instance.m_player4GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[3].ToString();

                UIElementReference.Instance.m_player1HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(0).ToString();
                UIElementReference.Instance.m_player2HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(1).ToString();
                UIElementReference.Instance.m_player3HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(2).ToString();
                UIElementReference.Instance.m_player4HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(3).ToString();

                //Enter Battle State
                print($"Local Client ID: {GameNetworkManager.Instance.GetPlayerID()}");
                m_papartionTimerCountDownTriggered = true;
                UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = "";
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_enterBattleStateSound);
            }
            else
            {
                //In Paparation State
                m_timerText -= Time.deltaTime;
                UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = Mathf.Floor(m_timerText).ToString();
            }
        }

        if (NetworkManager.Singleton.IsClient && !m_reposeTimerCountDownTriggered)
        {
            if (Time.time >= m_reposeTimer)
            {
                //Enter Next Paparation State
                m_reposeTimerCountDownTriggered = true;
                if (GameStateManager.Instance.GetGameTurn() % GameStateManager.Instance.GetAuctionStateTriggerCount() == 0)
                {
                    UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = "";
                }
                else
                {
                    UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = "";
                }
            }
            else
            {
                //In Repose State
                m_timerText -= Time.deltaTime;
                UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = Mathf.Floor(m_timerText).ToString();
            }
        }
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnPlayerModifyHealth.AddListener(OnPlayerModifyHealth);
        GameEventReference.Instance.OnPlayerModifyGold.AddListener(OnPlayerModifyGold);
        GameEventReference.Instance.OnStateChange.AddListener(OnStateChange);

        GameEventReference.Instance.OnEnterPreparationState.AddListener(OnEnterPreparationState);
        GameEventReference.Instance.OnEnterBattleState.AddListener(OnEnterBattleState);
        GameEventReference.Instance.OnEnterReposeState.AddListener(OnEnterReposeState);
        GameEventReference.Instance.OnEnemyDestroyed.AddListener(OnEnemyDestroyed);
        GameEventReference.Instance.OnEnterAuctionState.AddListener(OnEnterAuctionState);
    }

    private void OnEnterPreparationState(params object[] param)
    {
        m_stateText.Value = "Preparation";

    }

    private void OnEnterBattleState(params object[] param)
    {
        m_stateText.Value = "Battle";
    }

    private void OnEnterReposeState(params object[] param)
    {
        m_stateText.Value = "Repose";
    }

    private void OnEnterAuctionState(params object[] param)
    {
        OnEnterAuctionStateClientRpc();
    }
    [ClientRpc]
    private void OnEnterAuctionStateClientRpc()
    {
        UIElementReference.Instance.m_auctionPanel.SetActive(true);
        UIElementReference.Instance.m_player1GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[0].ToString();
        UIElementReference.Instance.m_player2GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[1].ToString();
        UIElementReference.Instance.m_player3GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[2].ToString();
        UIElementReference.Instance.m_player4GoldText.GetComponent<Text>().text = PlayerStatsManager.Instance.m_playersGoldList[3].ToString();
    }


    private void OnPlayerModifyHealth(params object[] param)
    {
        int newHealthAmount = (int)param[0];
        int modifierID = (int)param[1];
        ModifyHealthUIServerRpc(newHealthAmount, modifierID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ModifyHealthUIServerRpc(int healthAmount, int modifierID)
    {
        ModifyHealthUIClientRpc(healthAmount, modifierID);
    }

    [ClientRpc]
    private void ModifyHealthUIClientRpc(int healthAmount, int modifierID)
    {
        UIElementReference.Instance.m_player1HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(0).ToString();
        UIElementReference.Instance.m_player2HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(1).ToString();
        UIElementReference.Instance.m_player3HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(2).ToString();
        UIElementReference.Instance.m_player4HealthText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerHealth(3).ToString();
    }

    [ClientRpc]
    private void UpdatePlayerHealthClientRpc()
    {
        UIElementReference.Instance.m_mobsremainingText.GetComponent<TMP_Text>().text = EnemyManager.Instance.GetEnenyRemaining((int)NetworkManager.Singleton.LocalClientId).ToString();
    }


    private void OnPlayerModifyGold(params object[] param)
    {
        int newGoldAmount = (int)param[0];
        int modifierID = (int)param[1];
        ModifyGoldUIServerRpc(newGoldAmount, modifierID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ModifyGoldUIServerRpc(int goldAmount, int modifierID)
    {
        ModifyGoldUIClientRpc(goldAmount, modifierID);
    }

    [ClientRpc]
    private void ModifyGoldUIClientRpc(int goldAmount, int modifierID)
    {
        UIElementReference.Instance.m_player1StatusGoldText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerGold(0).ToString();
        UIElementReference.Instance.m_player2StatusGoldText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerGold(1).ToString();
        UIElementReference.Instance.m_player3StatusGoldText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerGold(2).ToString();
        UIElementReference.Instance.m_player4StatusGoldText.GetComponent<TMP_Text>().text = PlayerStatsManager.Instance.GetPlayerGold(3).ToString();
    }

    [ClientRpc]
    private void UpdatePlayerGoldClientRpc()
    {
        UIElementReference.Instance.m_mobsremainingText.GetComponent<TMP_Text>().text = EnemyManager.Instance.GetEnenyRemaining((int)NetworkManager.Singleton.LocalClientId).ToString();
    }

    private void OnStateChange(params object[] param)
    {
        GameState gameState = (GameState)param[0];
        UIElementReference.Instance.m_stateText.GetComponent<TMP_Text>().text = gameState.ToString();
    }

    private void OnEnemyDestroyed(params object[] param)
    {
        GameObject enemy = (GameObject)param[0];
        int mobMapID = (int)param[1];

        UpdateMobRemainingClientRpc();
    }

    [ClientRpc]
    private void UpdateMobRemainingClientRpc()
    {
        UIElementReference.Instance.m_mobsremainingText.GetComponent<TMP_Text>().text = EnemyManager.Instance.GetEnenyRemaining((int)NetworkManager.Singleton.LocalClientId).ToString();
    }

    private void UpdateStateText(FixedString4096Bytes previous, FixedString4096Bytes current)
    {
        UIElementReference.Instance.m_stateText.GetComponent<TMP_Text>().text = current.ToString();
        print(1);
        if (current.Equals("Preparation"))
        {
            print(2);
            UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = GameStateManager.Instance.GetPreparationTime().ToString();

            m_papartionTimerCountDownTriggered = false;

            m_preparationTimer = Time.time + GameStateManager.Instance.GetPreparationTime();
            m_timerText = GameStateManager.Instance.GetPreparationTime();
            print(3);
        }
        else if (current.Equals("Battle"))
        {
            UIElementReference.Instance.m_mobsremainingText.GetComponent<TMP_Text>().text = EnemyManager.Instance.GetEnenyRemaining(GameNetworkManager.Instance.GetPlayerID()).ToString();
            print($"GameNetworkManager.Instance.GetPlayerID() {GameNetworkManager.Instance.GetPlayerID()}");
        }
        else if (current.Equals("Repose"))
        {
            UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = GameStateManager.Instance.GetReposeTime().ToString();

            m_reposeTimerCountDownTriggered = false;

            m_reposeTimer = Time.time + GameStateManager.Instance.GetReposeTime();
            m_timerText = GameStateManager.Instance.GetReposeTime();
        }
    }
}
