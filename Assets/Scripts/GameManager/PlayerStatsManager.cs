using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerStatsManager : NetworkedSingleton<PlayerStatsManager>
{
    public int[] m_playersHealthList = new int[4];
    private const int m_playerHealthAmount = 50;
    public int[] m_playersGoldList = new int[4];
    private const int m_playerGoldAmount = 30;
    public int m_playerLosedAmount = 0;
    private bool m_isLosed = false;

    private NetworkVariable<int> m_losedAmount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private void Start()
    {
        Time.timeScale = 1f;

        for (int i = 0; i < m_playersHealthList.Length; i++)
        {
            m_playersHealthList[i] = m_playerHealthAmount;
        }

        for (int i = 0; i < m_playersGoldList.Length; i++)
        {
            m_playersGoldList[i] = m_playerGoldAmount;
        }
        SetUpListeners();
    }

    private void Update()
    {

    }

    private void LoseTrigger(int id)
    {
        LoseTriggerClientRpc(id);
    }

    [ClientRpc]
    private void LoseTriggerClientRpc(int id)
    {
        if (GameNetworkManager.Instance.GetPlayerID() == id)
        {
            print("Loseeeee");
        }
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
        if (m_isLosed) { return; }
        if (m_playersHealthList[modifierID] > 0)
            m_playersHealthList[modifierID] = newHealthAmount;

        if (GameNetworkManager.Instance.GetPlayerID() == modifierID)
        {
            if (m_playersHealthList[modifierID] <= 0)
            {
                m_isLosed = true;
                UIElementReference.Instance.m_loseGamePanel.SetActive(true);
                UIElementReference.Instance.m_restartButton.GetComponent<Button>().onClick.AddListener(delegate
                {
                    UIElementReference.Instance.m_loseGamePanel.SetActive(false);
                });

                UpdataServerPlayerLosedAmountServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdataServerPlayerLosedAmountServerRpc()
    {
        m_playerLosedAmount++;

        if (m_playerLosedAmount >= GameNetworkManager.Instance.GetPlayerNumber() - 1)
        {
            for (int i = 0; i < GameNetworkManager.Instance.GetPlayerNumber(); i++)
            {
                if (m_playersHealthList[i] > 0)
                {
                    int winPlayerIndex = 0;
                    EndGameTriggerClientRpc(winPlayerIndex);
                }
            }
        }
    }

    [ClientRpc]
    private void EndGameTriggerClientRpc(int id)
    {
        UIElementReference.Instance.m_EndGamePanel.SetActive(true);
        if (GameNetworkManager.Instance.GetPlayerID() == id)
        {
            UIElementReference.Instance.m_victoryGame.SetActive(true);
        }
        else
        {
            UIElementReference.Instance.m_loseGame.SetActive(true);
        }
        UIElementReference.Instance.m_restartGameButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            Disconnect();
            Cleanup();
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        });
        Time.timeScale = 0;
    }

    private void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void Cleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
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
        m_playersGoldList[modifierID] = newGoldAmount;
    }



    public int GetPlayerGold(int id) => m_playersGoldList[id];
    public int GetPlayerHealth(int id) => m_playersHealthList[id];
    public int SetPlayerGold(int newGoldAmount, int id) => m_playersGoldList[id] = newGoldAmount;
    public int SetPlayerHealth(int newHealthAmount, int id) => m_playersHealthList[id] = newHealthAmount;

    public bool GetLoseStatus() => m_isLosed;
}
