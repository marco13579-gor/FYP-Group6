using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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
        if (NetworkManager.Singleton.IsClient && !m_papartionTimerCountDownTriggered)
        {
            if (Time.time >= m_preparationTimer)
            {
                //Enter Battle State
                print($"Local Client ID: {GameNetworkManager.Instance.GetPlayerID()}");
                m_papartionTimerCountDownTriggered = true;
                UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = "EnteredBattleState";
                print("Changing EnteredBattleState text");
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
                //Enter Paparation State
                m_reposeTimerCountDownTriggered = true;
                UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = "EnteredParationState";
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
    }

    private void OnEnterPreparationState(params object[] param)
    {
        m_stateText.Value = "State: Preparation";
        
    }

    private void OnEnterBattleState(params object[] param)
    {
        m_stateText.Value = "State: Battle";
    }

    private void OnEnterReposeState(params object[] param)
    {
        m_stateText.Value = "State: Repose";
    }

    private void OnPlayerModifyHealth(params object[] param)
    {
        int newHealthAmount = (int)param[0];
        int modifierID = (int)param[1];
        UIElementReference.Instance.m_healthText.GetComponent<TMP_Text>().text = newHealthAmount.ToString();
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
        UIElementReference.Instance.m_goldText.GetComponent<TMP_Text>().text = newGoldAmount.ToString();
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
        if (current.Equals("State: Preparation"))
        {
            print(2);
            UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = GameStateManager.Instance.GetPreparationTime().ToString();

            m_papartionTimerCountDownTriggered = false;

            m_preparationTimer = Time.time + GameStateManager.Instance.GetPreparationTime();
            m_timerText = GameStateManager.Instance.GetPreparationTime();
            print(3);
        }
        else if (current.Equals("State: Battle"))
        {
            UIElementReference.Instance.m_mobsremainingText.GetComponent<TMP_Text>().text = EnemyManager.Instance.GetEnenyRemaining(GameNetworkManager.Instance.GetPlayerID()).ToString();
            print($"GameNetworkManager.Instance.GetPlayerID() {GameNetworkManager.Instance.GetPlayerID()}");
        }
        else if(current.Equals("State: Repose"))
        {
            UIElementReference.Instance.m_timerText.GetComponent<TMP_Text>().text = GameStateManager.Instance.GetReposeTime().ToString();

            m_reposeTimerCountDownTriggered = false;

            m_reposeTimer = Time.time + GameStateManager.Instance.GetReposeTime();
            m_timerText = GameStateManager.Instance.GetReposeTime();
        }
    }
}
