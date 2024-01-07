using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class TowerManager : NetworkedSingleton<TowerManager>
{
    public Dictionary<int, GameObject> m_towers = new Dictionary<int, GameObject>();

    [SerializeField]
    private Camera m_camera;

    private void Start()
    {
        UIElementReference.Instance.m_towerUpgradePanel.SetActive(false);

        SetUpListeners();
    }

    private void Update()
    {
        UpgradeTowerLogic();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnTowerPlaced.AddListener(OnTowerPlaced);
        GameEventReference.Instance.OnTowerChangeTarget.AddListener(OnTowerChangeTarget);
    }

    private void OnTowerPlaced(params object[] param)
    {
        int towerID = (int)param[0];
        TowerSO towerSO = (TowerSO)param[1];
        GameObject originTile = (GameObject)param[2];

        m_towers[towerID].GetComponent<Tower>().ToggleTowerIsPlaced(true);
        m_towers[towerID].GetComponent<Tower>().SetUsedTiles(0, originTile);

        for (int i = 0; i < towerSO.m_tileToBuild.Length; i++)
        {
            m_towers[towerID].GetComponent<Tower>().SetUsedTiles(i + 1, originTile.GetComponent<Tiles>().m_tilesNearby[towerSO.m_tileToBuild[i]].gameObject);
        }

        m_towers[towerID].GetComponent<TowerTargeting>().SetTower(m_towers[towerID].GetComponent<Tower>());
    }


    private void UpgradeTowerLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100000f, LayerMask.GetMask("Tower")) && Input.GetKeyDown(KeyCode.Mouse0) && BuildingManager.Instance.GetPrebuildTower() == null)
        {
            UIElementReference.Instance.m_towerUpgradePanel.SetActive(true);

            GameObject target = hit.collider.gameObject;

            UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());

            UIElementReference.Instance.m_towerUpgradeTowerAttackPowerButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (PlayerStatsManager.Instance.playersGoldList[GameNetworkManager.Instance.GetPlayerID()] < target.GetComponent<Tower>().GetUpgradeRequiredGold())
                    return;

                UpgradeTower(target.GetComponent<Tower>().GetTowerID(), UpgradeCore.AttackPower);

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - target.GetComponent<Tower>().GetUpgradeRequiredGold(), GameNetworkManager.Instance.GetPlayerID());

                target.GetComponent<Tower>().UpgradeGoldIncrease();

                UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            });

            UIElementReference.Instance.m_towerUpgradeTowerAttackSpeedButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (PlayerStatsManager.Instance.playersGoldList[GameNetworkManager.Instance.GetPlayerID()] < target.GetComponent<Tower>().GetUpgradeRequiredGold())
                    return;

                UpgradeTower(target.GetComponent<Tower>().GetTowerID(), UpgradeCore.AttackSpeed);

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - target.GetComponent<Tower>().GetUpgradeRequiredGold(), GameNetworkManager.Instance.GetPlayerID());

                target.GetComponent<Tower>().UpgradeGoldIncrease();

                UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            });

            UIElementReference.Instance.m_towerUpgradeTowerAttackRangeButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (PlayerStatsManager.Instance.playersGoldList[GameNetworkManager.Instance.GetPlayerID()] < target.GetComponent<Tower>().GetUpgradeRequiredGold())
                    return;

                UpgradeTower(target.GetComponent<Tower>().GetTowerID(), UpgradeCore.AttackRange);

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - target.GetComponent<Tower>().GetUpgradeRequiredGold(), GameNetworkManager.Instance.GetPlayerID());

                target.GetComponent<Tower>().UpgradeGoldIncrease();

                UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            });
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            UIElementReference.Instance.m_towerUpgradeTowerAttackPowerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerUpgradeTowerAttackSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerUpgradeTowerAttackRangeButton.GetComponent<Button>().onClick.RemoveAllListeners();

            UIElementReference.Instance.m_towerUpgradePanel.SetActive(false);
        }
    }

    private void UpdateUpgradeGoldText(int goldToUpgrade)
    {
        UIElementReference.Instance.m_towerUpgradeTowerAttackPowerText.GetComponent<TMP_Text>().text = $"${goldToUpgrade.ToString()}";
        UIElementReference.Instance.m_towerUpgradeTowerAttackSpeedText.GetComponent<TMP_Text>().text = $"${goldToUpgrade.ToString()}";
        UIElementReference.Instance.m_towerUpgradeTowerAttackRangeText.GetComponent<TMP_Text>().text = $"${goldToUpgrade.ToString()}";
    }

    private void UpgradeTower(params object[] param)
    {
        int towerID = (int)param[0];
        UpgradeCore upgradeCore = (UpgradeCore)param[1];

        switch (upgradeCore)
        {
            case UpgradeCore.AttackPower:
                m_towers[towerID].GetComponent<Tower>().UpgradeCoreAttackPower();
                break;
            case UpgradeCore.AttackSpeed:
                m_towers[towerID].GetComponent<Tower>().UpgradeCoreAttackSpeed();
                break;
            case UpgradeCore.AttackRange:
                m_towers[towerID].GetComponent<Tower>().UpgradeCoreAttackRange();
                break;
        }
    }

    private void OnTowerChangeTarget(params object[] param)
    {
        int towerID = (int)param[0];
        Enemy targetEnemy = (Enemy)param[1];

        m_towers[towerID].GetComponent<Tower>().SetTargetEnemy(targetEnemy);
    }

    public GameObject GetTower(int index) => m_towers[index];

    public int Register(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Tower tower))
        {
            //Detect if the list was full
            if (m_towers.Count >= 16777217)
            {
                Debug.LogError("Failed To Register Tower: " + gameObj.name + "\nTower List Is Full!");
            }

            //arrange valid index
            int randomIndex = Random.Range(0, 16777216);
            while (m_towers.ContainsKey(randomIndex))
            {
                randomIndex = Random.Range(0, 16777216);
            }

            //Add to list
            AddTowerToListClientRpc(gameObj.GetComponent<Tower>(), randomIndex);
            //m_towers.Add(randomIndex, gameObj);
            gameObj.GetComponent<Tower>().AssignID(randomIndex);

            return randomIndex;
        }
        else
        {
            Debug.LogError("Invalid Tower: " + gameObj.name);
            return -1;
        }
    }

    [ClientRpc]
    private void AddTowerToListClientRpc(NetworkBehaviourReference towerToAddToList, int towerID)
    {
        Tower tower;
        towerToAddToList.TryGet(out tower);

        GameObject towerGameObject = tower.gameObject;

        m_towers.Add(towerID, towerGameObject);
    }

    [ClientRpc]
    private void DeleteTowerToListClientRpc(int towerID)
    {
        m_towers.Remove(towerID);
    }

    public void Unregister(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Tower tower))
        {
            DeleteTowerToListClientRpc(tower.GetTowerID());
            //m_towers.Remove(tower.GetTowerID());
        }
        else
        {
            Debug.LogError("Invalid Tower: " + gameObj.name);
        }
    }
}
