using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Unity.Netcode;

public class CardSlotManager : NetworkedSingleton<CardSlotManager>
{
    [SerializeField]
    private GameObject[] m_cardSlots;

    private const int m_cardSlotAmount = 5;
    private int m_randomIndex;

    private void Start()
    {
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnEnterPreparationState.AddListener(OnEnterPreparationState);
        GameEventReference.Instance.OnPlayerConsumeCard.AddListener(OnPlayerConsumeCard);

        GameEventReference.Instance.OnAuctionEnd.AddListener(OnAuctionEnd);
    }

    private void RefreshCardSlot(int slotIndex)
    {
        if (PlayerStatsManager.Instance.GetLoseStatus()) return;

        m_cardSlots[slotIndex].SetActive(true);
        m_cardSlots[slotIndex].transform.GetChild(4).GetComponent<Button>().onClick.RemoveAllListeners();

        int randomIndex = GetRandomCardIndex(GameStateManager.Instance.GetGameTurn());

        m_cardSlots[slotIndex].transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate
        {
            if (BuildingManager.Instance.GetPrebuildTower() == null)
            {
                BuildingManager.Instance.SelectTower(CardDatabaseReference.Instance.m_cardDatabaseList[randomIndex].GetComponent<PrebuildTower>());
                BuildingManager.Instance.SetCardConsumeSlot(slotIndex);
            }
        });

        UpdataCardSlotVisual(slotIndex, CardDatabaseReference.Instance.m_cardDatabaseList[randomIndex].GetComponent<PrebuildTower>());
    }

    private void OnAuctionEnd(params object[] param)
    {
        int rewardCardIndex = (int)param[0];
        RefreshAuctionCardSlot(rewardCardIndex);
    }

    private void RefreshAuctionCardSlot(int rewardCardIndex)
    {
        m_cardSlots[5].SetActive(true);
        m_cardSlots[5].transform.GetChild(4).GetComponent<Button>().onClick.RemoveAllListeners();

        m_cardSlots[5].transform.GetChild(4).GetComponent<Button>().onClick.AddListener(delegate
        {
            if (BuildingManager.Instance.GetPrebuildTower() == null)
            {
                BuildingManager.Instance.SelectTower(CardDatabaseReference.Instance.m_auctionCardDatabaseList[rewardCardIndex].GetComponent<PrebuildTower>());
                BuildingManager.Instance.SetCardConsumeSlot(5);
            }
        });

        UpdataCardSlotVisual(5, CardDatabaseReference.Instance.m_auctionCardDatabaseList[rewardCardIndex].GetComponent<PrebuildTower>());
    }

    private void OnPlayerConsumeCard(params object[] param)
    {
        int cardIndex = (int)param[0];
        RemoveCardSlotContent(cardIndex);
    }

    private void OnEnterPreparationState(params object[] param)
    {
        RefreshCardSlotClientRpc();
    }

    [ClientRpc]
    private void RefreshCardSlotClientRpc()
    {
        for (int i = 0; i < m_cardSlotAmount; i++)
        {
            RefreshCardSlot(i);
        }
    }

    public void UpdataCardSlotVisual(int slotIndex, PrebuildTower towerToBuild)
    {
        m_cardSlots[slotIndex].transform.GetChild(0).GetComponent<Image>().sprite = towerToBuild.m_towerSO.m_sprite;
        m_cardSlots[slotIndex].transform.GetChild(2).GetComponent<TMP_Text>().text = towerToBuild.m_towerSO.m_name;
        m_cardSlots[slotIndex].transform.GetChild(3).GetComponent<TMP_Text>().text = towerToBuild.m_towerSO.m_desritption;
        m_cardSlots[slotIndex].transform.GetChild(5).GetComponent<TMP_Text>().text = towerToBuild.m_towerSO.m_cost.ToString();
        m_cardSlots[slotIndex].transform.GetChild(6).GetComponent<TMP_Text>().text = towerToBuild.m_towerSO.m_attackPower.ToString();
        m_cardSlots[slotIndex].transform.GetChild(7).GetComponent<TMP_Text>().text = towerToBuild.m_towerSO.m_attackRange.ToString();
    }

    private void RemoveCardSlotContent(int cardSlotToRemove)
    {
        m_cardSlots[cardSlotToRemove].transform.GetChild(4).GetComponent<Button>().onClick.RemoveAllListeners();
        m_cardSlots[cardSlotToRemove].SetActive(false);
    }

    private int GetRandomCardIndex(int turnValue)
    {
        if (turnValue == 0)
        {
            turnValue = 1;
        }
        CardDrawState cardDrawState;
        if (turnValue <= 5)
        {
            cardDrawState = CardDrawState.EarlyGame;
        }
        else if (turnValue <= 12)
        {
            cardDrawState = CardDrawState.MidGame;
        }
        else
        {
            cardDrawState = CardDrawState.LateGame;
        }

        int drawValue = 0;
        switch (cardDrawState)
        {
            case CardDrawState.EarlyGame:
                drawValue = 15;
                break;
            case CardDrawState.MidGame:
                drawValue = 25; 
                break;
            case CardDrawState.LateGame:
                drawValue = 40; 
                break;
        }

        do
        {
            m_randomIndex = UnityEngine.Random.Range(0, CardDatabaseReference.Instance.m_cardDatabaseList.Length);

        } while (drawValue < CardDatabaseReference.Instance.m_cardDatabaseList[m_randomIndex].GetComponent<PrebuildTower>().m_towerSO.m_cost);
        return m_randomIndex;
    }
}
