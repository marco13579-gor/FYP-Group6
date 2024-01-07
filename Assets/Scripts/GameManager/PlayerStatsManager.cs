using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStatsManager : NetworkedSingleton<PlayerStatsManager>
{
    public int[] playersHealthList = new int[4];
    private const int playerHealthAmount = 50;
    public int[] playersGoldList = new int[4];
    private const int playerGoldAmount = 50;

    private void Start()
    {
        for (int i = 0; i < playersHealthList.Length; i++)
        {
            playersHealthList[i] = playerHealthAmount;
        }

        for (int i = 0; i < playersGoldList.Length; i++)
        {
            playersGoldList[i] = playerGoldAmount;
        }
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnPlayerModifyHealth.AddListener(OnPlayerModifyHealth);
        GameEventReference.Instance.OnPlayerModifyGold.AddListener(OnPlayerModifyGold);
    }

    private void OnPlayerModifyHealth(params object[] param)
    {
        int newHealthAmount = (int)param[0];
        int modifierID = (int)param[1];
        UpdatePlayerHealthModifyServerRpc(newHealthAmount, modifierID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerHealthModifyServerRpc(int newHealthAmount, int modifierID)
    {
        UpdatePlayerHealthModifyClientRpc(newHealthAmount, modifierID);
    }

    [ClientRpc]
    private void UpdatePlayerHealthModifyClientRpc(int newHealthAmount, int modifierID)
    {
         playersHealthList[modifierID] = newHealthAmount;
    }

    private void OnPlayerModifyGold(params object[] param)
    {
        int newGoldAmount = (int)param[0];
        int modifierID = (int)param[1];
        UpdatePlayerGoldModifyServerRpc(newGoldAmount, modifierID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerGoldModifyServerRpc(int newGoldAmount, int modifierID)
    {
        print("UpdatePlayerGoldModifyServerRpc");
        UpdatePlayerGoldModifyClientRpc(newGoldAmount, modifierID);
    }
    [ClientRpc]
    private void UpdatePlayerGoldModifyClientRpc(int newGoldAmount, int modifierID)
    {
        print($"Modify Target ID: {modifierID}");
        playersGoldList[modifierID] = newGoldAmount;
    }



    public int GetPlayerGold(int id) => playersGoldList[id];
    public int GetPlayerHealth(int id) => playersHealthList[id];
    public int SetPlayerGold(int newGoldAmount, int id) => playersGoldList[id] = newGoldAmount;
    public int SetPlayerHealth(int newHealthAmount, int id) => playersHealthList[id] = newHealthAmount;
}
