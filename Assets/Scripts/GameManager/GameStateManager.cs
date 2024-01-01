using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameStateManager : NetworkedSingleton<GameStateManager>
{
    private bool m_firstTurnTriggered = false;

    private NetworkVariable<GameState> m_gameState = new NetworkVariable<GameState>(GameState.Battle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float m_enterStateTime;

    private const float m_prepartionTime = 6f;
    private const float m_reposeTime = 4f;

    private bool m_enterBattleStateTrigger = true;
    private bool m_enterPaparationStateTrigger = true;

    private bool m_isReadyButtonClicked = false;

    private int m_turn;
    private void Start()
    {
        SetUpListeners();
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer && !m_firstTurnTriggered && m_isReadyButtonClicked)
        {
            GameEventReference.Instance.OnEnterPreparationState.Trigger();
            m_firstTurnTriggered = true;
        }

        //CountDown for entering battle state
        if(NetworkManager.Singleton.IsServer && !m_enterBattleStateTrigger)
        {
            if (Time.time >= m_enterStateTime)
            {
                if(WaveDatabaseReference.Instance.m_waveList[m_turn] != null)
                {
                    GameEventReference.Instance.OnEnterBattleState.Trigger(WaveDatabaseReference.Instance.m_waveList[m_turn]);
                    ++m_turn;
                    m_enterBattleStateTrigger = true;
                }
                else
                {
                    Debug.LogError("Wave Size out of Bound!");
                }
            }
        }

        //CountDown for entering paparation state
        if(NetworkManager.Singleton.IsServer && !m_enterPaparationStateTrigger)
        {
            if (Time.time >= m_enterStateTime)
            {
                GameEventReference.Instance.OnEnterPreparationState.Trigger();
                m_enterPaparationStateTrigger = true;
            }
        }
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnEnterPreparationState.AddListener(OnEnterPrepartionState);
        GameEventReference.Instance.OnEnterBattleState.AddListener(OnEnterBattleState);
        GameEventReference.Instance.OnEnterReposeState.AddListener(OnEnterReposeState);
    }

    private void OnEnterPrepartionState(params object[] param)
    {
        m_enterStateTime = Time.time + m_prepartionTime;
        print("Time added");
        print($"m_enterStateTime: {m_enterStateTime} Time: {Time.time}");
        m_enterBattleStateTrigger = false;
        print("OnEnterPrepartionState");
    }

    private void OnEnterBattleState(params object[] param)
    {
        print("OnEnterBattleState");
    }
    
    private void OnEnterReposeState(params object[] param)
    {
        print("OnEnterReposeState");

        //give player gold
        for(int i = 0; i < GameNetworkManager.Instance.GetPlayerNumber(); i++)
        {
            int newgold = PlayerStatsManager.Instance.GetPlayerGold(i) + 10;
            GameEventReference.Instance.OnPlayerModifyGold.Trigger(newgold, i);
        }
        m_enterStateTime = 5 + Time.time;
        m_enterPaparationStateTrigger = false;
    }

    public GameState GetGameState() => m_gameState.Value;

    public float GetPreparationTime() => m_prepartionTime;

    public float GetReposeTime() => m_reposeTime;

    public void ToggleReadyButtonClick() => m_isReadyButtonClicked = true;

    public int GetGameTurn() => m_turn;
}
