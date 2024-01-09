using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public class AuctionManager : NetworkedSingleton<AuctionManager>
{
    [SerializeField]
    private GameObject m_auctionCardSlot;

    private bool m_isAuctionStateFirstTrigger = false;
    private bool m_isTurnChangeTrigger = false;
    private NetworkVariable<int> m_currentPlayerTurn = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> m_highestBid = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private int m_bidAmount = 0;
    private const int m_minimumBidAmount = 10;

    private bool[] m_playerGiveUpList;
    private int m_playersGaveUp = 0;

    private void Start()
    {
        SetUpListeners();
    }

    private void Update()
    {
        if (!IsServer) { return; }

        if (m_isAuctionStateFirstTrigger)
        {
            m_playerGiveUpList = new bool[NetworkManager.Singleton.ConnectedClients.Count];
            m_isAuctionStateFirstTrigger = false;
        }

        if (m_isTurnChangeTrigger)
        {
            if (m_currentPlayerTurn.Value == GameNetworkManager.Instance.GetPlayerNumber())
            {
                m_currentPlayerTurn.Value = 0;
            }

            TurnChangeClientRpc(m_currentPlayerTurn.Value);

            ++m_currentPlayerTurn.Value;

            m_isTurnChangeTrigger = false;
        }
    }

    [ClientRpc]
    private void TurnChangeClientRpc(int playerTurnIndex)
    {
        if (playerTurnIndex == GameNetworkManager.Instance.GetPlayerID())
        {
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


    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnEnterAuctionState.AddListener(OnEnterAuctionState);
    }

    private void OnEnterAuctionState(params object[] param)
    {
        m_highestBid.Value = 10;

        m_isTurnChangeTrigger = true;
        m_isAuctionStateFirstTrigger = true;
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
        int winPlayerIndex;
        m_playerGiveUpList[playerID] = false;
        ++m_playersGaveUp;

        if (m_playersGaveUp >= GameNetworkManager.Instance.GetPlayerNumber() - 1)
        {
            for (int i = 0; i < m_playerGiveUpList.Length; i++)
            {
                if (m_playerGiveUpList[i] == true)
                {
                    winPlayerIndex = i;
                    EndAuctionState(winPlayerIndex);
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
        EndAuctionStateClientRpc(winPlayerIndex);
    }

    [ClientRpc]
    private void EndAuctionStateClientRpc(int winPlayerIndex)
    {
        UIElementReference.Instance.m_auctionPanel.SetActive(false);
        if (winPlayerIndex == GameNetworkManager.Instance.GetPlayerID())
        {
            print("Auciton win");
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
