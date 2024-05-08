using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class AuctionManager : NetworkedSingleton<AuctionManager>
{
    private bool m_isAuctionStateFirstTrigger = false;
    private bool m_isTurnChangeTrigger = false;
    private NetworkVariable<int> m_currentPlayerTurn = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> m_highestBid = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private int m_bidAmount = 0;
    private const int m_minimumBidAmount = 10;

    private bool[] m_playerGiveUpList;
    private int m_playersGaveUp = 0;
    private int m_rewardCardIndex;

    private void Start()
    {
        SetUpListeners();
    }

    private void Update()
    {
        if (!IsServer) { return; }

        if (m_isAuctionStateFirstTrigger)
        {
            EndAuctionState(0);

            m_playerGiveUpList = new bool[NetworkManager.Singleton.ConnectedClients.Count];
            m_isAuctionStateFirstTrigger = false;

            switch (GameNetworkManager.Instance.GetPlayerNumber())
            {
                case 1:
                    UIElementReference.Instance.m_player1GoldText.SetActive(true);
                    UIElementReference.Instance.m_player2GoldText.SetActive(false);
                    UIElementReference.Instance.m_player3GoldText.SetActive(false);
                    UIElementReference.Instance.m_player4GoldText.SetActive(false);
                    break;
                case 2:
                    UIElementReference.Instance.m_player1GoldText.SetActive(true);
                    UIElementReference.Instance.m_player2GoldText.SetActive(true);
                    UIElementReference.Instance.m_player3GoldText.SetActive(false);
                    UIElementReference.Instance.m_player4GoldText.SetActive(false);
                    break;
                case 3:
                    UIElementReference.Instance.m_player1GoldText.SetActive(true);
                    UIElementReference.Instance.m_player2GoldText.SetActive(true);
                    UIElementReference.Instance.m_player3GoldText.SetActive(true);
                    UIElementReference.Instance.m_player4GoldText.SetActive(false);
                    break;
            }
        }

        if (m_isTurnChangeTrigger)
        {
            if (m_currentPlayerTurn.Value == GameNetworkManager.Instance.GetPlayerNumber())
            {
                m_currentPlayerTurn.Value = 0;
            }

            TurnChangeClientRpc(m_currentPlayerTurn.Value, m_currentPlayerTurn.Value, m_highestBid.Value);

            ++m_currentPlayerTurn.Value;

            m_isTurnChangeTrigger = false;
        }
    }

    [ClientRpc]
    private void TurnChangeClientRpc(int playerTurnIndex, int playerTurn, int highestBidAmount)
    {
        if (playerTurnIndex == GameNetworkManager.Instance.GetPlayerID())
        {
            if (PlayerStatsManager.Instance.GetLoseStatus())
            {
                OnGiveUpButtonClicked();
                return;
            }

            UIElementReference.Instance.m_bidButton.GetComponent<Button>().interactable = true;
            UIElementReference.Instance.m_giveupButton.GetComponent<Button>().interactable = true;

            UIElementReference.Instance.m_bidButton.GetComponent<Button>().onClick.AddListener(OnBidButtonClicked);
            UIElementReference.Instance.m_giveupButton.GetComponent<Button>().onClick.AddListener(OnGiveUpButtonClicked);
            UIElementReference.Instance.m_bidAmountInputField.GetComponent<InputField>().onEndEdit.AddListener(OnBidAmountInputChange);
        }
        else
        {
            UIElementReference.Instance.m_bidButton.GetComponent<Button>().interactable = false;
            UIElementReference.Instance.m_giveupButton.GetComponent<Button>().interactable = false;

            UIElementReference.Instance.m_bidButton.GetComponent<Button>().onClick.RemoveListener(OnBidButtonClicked);
            UIElementReference.Instance.m_giveupButton.GetComponent<Button>().onClick.RemoveListener(OnGiveUpButtonClicked);
            UIElementReference.Instance.m_bidAmountInputField.GetComponent<InputField>().onEndEdit.RemoveListener(OnBidAmountInputChange);
        }

        UIElementReference.Instance.m_currentBidderText.GetComponent<Text>().text = playerTurn.ToString();
        UIElementReference.Instance.m_bidAmountText.GetComponent<Text>().text = highestBidAmount.ToString();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnEnterAuctionState.AddListener(OnEnterAuctionState);
    }

    private void OnEnterAuctionState(params object[] param)
    {
        m_rewardCardIndex = UnityEngine.Random.Range(0, CardDatabaseReference.Instance.m_auctionCardDatabaseList.Length);
        UpdataAuctionCardClientRpc(m_rewardCardIndex);

        m_currentPlayerTurn.Value = 0;
        m_highestBid.Value = 10;
        m_playersGaveUp = 0;

        m_isTurnChangeTrigger = true;
        m_isAuctionStateFirstTrigger = true;
    }

    [ClientRpc]
    private void UpdataAuctionCardClientRpc(int rewardCardIndex)
    {
        if (PlayerStatsManager.Instance.GetLoseStatus()) return;

        PrebuildTower towerToReward = CardDatabaseReference.Instance.m_auctionCardDatabaseList[rewardCardIndex].GetComponent<PrebuildTower>();

        UIElementReference.Instance.m_auctionCardSlot.transform.GetChild(0).GetComponent<Image>().sprite = towerToReward.m_towerSO.m_sprite;
        UIElementReference.Instance.m_auctionCardSlot.transform.GetChild(2).GetComponent<TMP_Text>().text = towerToReward.m_towerSO.m_name;
        UIElementReference.Instance.m_auctionCardSlot.transform.GetChild(3).GetComponent<TMP_Text>().text = towerToReward.m_towerSO.m_desritption;
        UIElementReference.Instance.m_auctionCardSlot.transform.GetChild(5).GetComponent<TMP_Text>().text = towerToReward.m_towerSO.m_cost.ToString();
        UIElementReference.Instance.m_auctionCardSlot.transform.GetChild(6).GetComponent<TMP_Text>().text = towerToReward.m_towerSO.m_attackPower.ToString();
        UIElementReference.Instance.m_auctionCardSlot.transform.GetChild(7).GetComponent<TMP_Text>().text = towerToReward.m_towerSO.m_attackRange.ToString();
    }

    public void OnBidButtonClicked()
    {
        if (m_highestBid.Value == 0)
        {
            print("input the bid amount");
            return;
        }

        if (m_highestBid.Value + m_minimumBidAmount > m_bidAmount)
        {
            print($"bid amount must be highest + {m_minimumBidAmount}");
            return;
        }

        if(m_bidAmount > PlayerStatsManager.Instance.m_playersGoldList[GameNetworkManager.Instance.GetPlayerID()])
        {
            print($"You don't have enough money!");
            return;
        }

        BidButtonClickedServerRpc(m_bidAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void BidButtonClickedServerRpc(int bidAmount)
    {
        m_highestBid.Value = bidAmount;
        m_isTurnChangeTrigger = true;
    }

    public void OnGiveUpButtonClicked()
    {
        int playerID = GameNetworkManager.Instance.GetPlayerID();
        GiveUpButtonClickedServerRpc(playerID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GiveUpButtonClickedServerRpc(int playerID)
    {
        m_playerGiveUpList[playerID] = true;
        ++m_playersGaveUp;

        if (m_playersGaveUp >= GameNetworkManager.Instance.GetPlayerNumber() - 1)
        {
            for (int i = 0; i < m_playerGiveUpList.Length; i++)
            {
                print(m_playerGiveUpList[i]);
                if (m_playerGiveUpList[i] == false)
                { 
                    EndAuctionState(i);
                    return;
                }
            }
        }
        else
        {
            m_isTurnChangeTrigger = true;
        }
    }

    private void EndAuctionState(int winPlayerIndex)
    {
        EndAuctionStateClientRpc(winPlayerIndex, m_rewardCardIndex);
        GameEventReference.Instance.OnEnterPreparationState.Trigger();
    }

    [ClientRpc]
    private void EndAuctionStateClientRpc(int winPlayerIndex, int rewardCardIndex)
    {
        UIElementReference.Instance.m_auctionPanel.SetActive(false);
        if (winPlayerIndex == GameNetworkManager.Instance.GetPlayerID())
        {
            GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - m_highestBid.Value, GameNetworkManager.Instance.GetPlayerID());
            GameEventReference.Instance.OnAuctionEnd.Trigger(rewardCardIndex);
        }
        else
        {
            print("You lost");
        }
    }

    public void OnBidAmountInputChange(string s)
    {
        m_bidAmount = int.Parse(s);
    }
}
