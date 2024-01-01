using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardSlotManager : Singleton<CardSlotManager>
{
    [SerializeField]
    private GameObject m_cardButton1;
    [SerializeField]
    private GameObject m_cardButton2;
    [SerializeField]
    private GameObject m_cardButton3;
    [SerializeField]
    private GameObject m_cardButton4;
    [SerializeField]
    private GameObject m_cardButton5;

    private int m_randomIndex;
    public void RefreshCardSlot(GameObject cardSlot)
    {
        int randomCardIndex = GetRandomCardIndex(GameStateManager.Instance.GetGameTurn());

        int towerCost = CardDatabaseReference.Instance.m_cardDatabaseList[randomCardIndex].GetComponent<PrebuildTower>().m_towerSO.m_cost;

        cardSlot.GetComponent<Button>().onClick.AddListener(delegate
        {
            BuildingManager.Instance.SelectTower(CardDatabaseReference.Instance.m_cardDatabaseList[randomCardIndex].GetComponent<PrebuildTower>());
        });
    }

    private int GetRandomCardIndex(int turnValue)
    {
        do {
            m_randomIndex = UnityEngine.Random.Range(0, CardDatabaseReference.Instance.m_cardDatabaseList.Length);

        } while (turnValue < CardDatabaseReference.Instance.m_cardDatabaseList[m_randomIndex].GetComponent<PrebuildTower>().m_towerSO.m_cost);
        return m_randomIndex;
    }
}
