using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class TowerManager : NetworkedSingleton<TowerManager>
{
    public Dictionary<int, GameObject> m_towers = new Dictionary<int, GameObject>();

    [SerializeField]
    private Camera m_camera;
    [SerializeField]
    private Material m_outlineMaterial;
    [SerializeField]
    private Shader m_outlineShader;

    private GameObject m_selectedTower;
    private GameObject m_previousSelectedTower;

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
        GameEventReference.Instance.OnTowerRemoved.AddListener(OnTowerRemoved);
    }

    private void OnTowerPlaced(params object[] param)
    {
        int towerID = (int)param[0];
        TowerSO towerSO = (TowerSO)param[1];
        GameObject originTile = (GameObject)param[2];

        SetTowerTileListServerRpc(towerSO.m_tileToBuild, towerID, originTile.GetComponent<Tiles>());

        m_towers[towerID].GetComponent<TowerTargeting>().SetTower(m_towers[towerID].GetComponent<Tower>());
    }

    private void OnTowerRemoved(params object[] param)
    {
        GameObject[] usedTiles = (GameObject[])param[1];
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTowerTileListServerRpc(int[] m_tileToBuild, int towerID, NetworkBehaviourReference tilesParam)
    {
        Tiles originTile;
        tilesParam.TryGet(out originTile);

        SetTowerTileListClientRpc(m_tileToBuild, towerID, originTile.GetComponent<Tiles>());
    }

    [ClientRpc]
    private void SetTowerTileListClientRpc(int[] m_tileToBuild, int towerID, NetworkBehaviourReference tilesParam)
    {
        Tiles originTile;
        tilesParam.TryGet(out originTile);

        m_towers[towerID].GetComponent<Tower>().ToggleTowerIsPlaced(true);
        m_towers[towerID].GetComponent<Tower>().SetUsedTiles(0, originTile.gameObject);

        for (int i = 0; i < m_tileToBuild.Length; i++)
        {
            m_towers[towerID].GetComponent<Tower>().SetUsedTiles(i + 1, originTile.m_tilesNearby[m_tileToBuild[i]].gameObject);
        }
    }


    private void UpgradeTowerLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100000f, LayerMask.GetMask("Tower")) && Input.GetKeyDown(KeyCode.Mouse0) && BuildingManager.Instance.GetPrebuildTower() == null && m_selectedTower == null)
        {
            if (hit.transform.gameObject.GetComponent<Tower>().m_usedTiles[0].GetComponent<Tiles>().GetPossibleBuilderID() != GameNetworkManager.Instance.GetPlayerID())
            {
                return;
            }

            //Turn off last selected tower RangeIndiactor
            if (m_selectedTower != null)
                m_selectedTower.GetComponent<Tower>().TurnOffRangeIndiactor();

            UIElementReference.Instance.m_towerUpgradePanel.SetActive(true);

            GameObject currentTarget = null;
            GameObject target = m_selectedTower;
            float minDistance = 100;

            Collider[] targets = Physics.OverlapSphere(hit.point, 5f, LayerMask.GetMask("Tower"));
            for (int i = 0; i < targets.Length; i++)
            {
                GameObject go = targets[i].gameObject;
                if (Vector3.Distance(go.transform.position, hit.point) < minDistance)
                {
                    currentTarget = go;
                    minDistance = Vector3.Distance(go.transform.position, hit.point);
                }
            }

            Material mat = new Material(m_outlineMaterial);
            mat.SetColor("_Color", Color.blue);
            mat.SetFloat("_Scale", 1.05f);
            if (target != null)
            {
                m_previousSelectedTower = m_selectedTower;
            }
            target = currentTarget;
            m_selectedTower = currentTarget;


            target.GetComponent<Tower>().TurnOnRangeIndiactor();

            UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            UIElementReference.Instance.m_towerImage.GetComponent<Image>().sprite = target.GetComponent<Tower>().m_towerSO.m_sprite;
            UIElementReference.Instance.m_attackPowerText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().GetAttackPower().ToString();
            UIElementReference.Instance.m_attackRangeText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().GetAttackRange().ToString();
            UIElementReference.Instance.m_attacSpeedText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().GetAttackSpeed().ToString();
            UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            UIElementReference.Instance.m_desriptionText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().m_towerSO.m_desritption.ToString();

            UIElementReference.Instance.m_towerUpgradeTowerAttackPowerButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (PlayerStatsManager.Instance.m_playersGoldList[GameNetworkManager.Instance.GetPlayerID()] < target.GetComponent<Tower>().GetUpgradeRequiredGold())
                {
                    GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_wrongSound);
                    return;
                }

                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_upgradeSound);

                UpgradeTower(target.GetComponent<Tower>().GetTowerID(), UpgradeCore.AttackPower);

                target.GetComponent<Tower>().EnableUpgradeEffect();

                UIElementReference.Instance.m_attackPowerText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().GetAttackPower().ToString();

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - target.GetComponent<Tower>().GetUpgradeRequiredGold(), GameNetworkManager.Instance.GetPlayerID());

                target.GetComponent<Tower>().UpgradeGoldIncrease();

                UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            });

            UIElementReference.Instance.m_towerUpgradeTowerAttackSpeedButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (PlayerStatsManager.Instance.m_playersGoldList[GameNetworkManager.Instance.GetPlayerID()] < target.GetComponent<Tower>().GetUpgradeRequiredGold())
                {
                    GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_wrongSound);
                    return;
                }

                UpgradeTower(target.GetComponent<Tower>().GetTowerID(), UpgradeCore.AttackSpeed);

                UIElementReference.Instance.m_attacSpeedText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().GetAttackSpeed().ToString();

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - target.GetComponent<Tower>().GetUpgradeRequiredGold(), GameNetworkManager.Instance.GetPlayerID());

                target.GetComponent<Tower>().UpgradeGoldIncrease();

                UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            });

            UIElementReference.Instance.m_towerUpgradeTowerAttackRangeButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                if (PlayerStatsManager.Instance.m_playersGoldList[GameNetworkManager.Instance.GetPlayerID()] < target.GetComponent<Tower>().GetUpgradeRequiredGold())
                {
                    GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_wrongSound);
                    return;
                }

                UpgradeTower(target.GetComponent<Tower>().GetTowerID(), UpgradeCore.AttackRange);

                UIElementReference.Instance.m_attackRangeText.GetComponent<TMP_Text>().text = target.GetComponent<Tower>().GetAttackRange().ToString();

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - target.GetComponent<Tower>().GetUpgradeRequiredGold(), GameNetworkManager.Instance.GetPlayerID());

                target.GetComponent<Tower>().UpgradeGoldIncrease();

                UpdateUpgradeGoldText(target.GetComponent<Tower>().GetUpgradeRequiredGold());
            });

            UIElementReference.Instance.m_towerTargetConditionFirstTargetButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                target.GetComponent<TowerTargeting>().SetTowerTargetCondition(TowerTargetsSelectCondition.FirstTarget);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_towerTargetSelectChangeSound);
                UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            });

            UIElementReference.Instance.m_towerTargetConditionLastTargetButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                target.GetComponent<TowerTargeting>().SetTowerTargetCondition(TowerTargetsSelectCondition.LastTarget);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_towerTargetSelectChangeSound);
                UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            });

            UIElementReference.Instance.m_towerTargetConditionMaxHealthButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                target.GetComponent<TowerTargeting>().SetTowerTargetCondition(TowerTargetsSelectCondition.MaxHealth);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_towerTargetSelectChangeSound);
                UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            });

            UIElementReference.Instance.m_towerTargetConditionMinHealthButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                target.GetComponent<TowerTargeting>().SetTowerTargetCondition(TowerTargetsSelectCondition.MinHealth);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_towerTargetSelectChangeSound);
                UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            });

            UIElementReference.Instance.m_towerTargetConditionMaxSpeedButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                target.GetComponent<TowerTargeting>().SetTowerTargetCondition(TowerTargetsSelectCondition.MaxSpeed);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_towerTargetSelectChangeSound);
                UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            });

            UIElementReference.Instance.m_towerTargetConditionMinSpeedButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                target.GetComponent<TowerTargeting>().SetTowerTargetCondition(TowerTargetsSelectCondition.MinSpeed);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_towerTargetSelectChangeSound);
                UIElementReference.Instance.m_currentAtkMode.GetComponent<TMP_Text>().text = target.GetComponent<TowerTargeting>().TargetConditionToString();
            });

            UIElementReference.Instance.m_removeTowerButton.GetComponent<Button>().onClick.AddListener(delegate
            {
                GameEventReference.Instance.OnTowerRemoved.Trigger(m_selectedTower.GetComponent<Tower>().GetTowerID(), m_selectedTower.GetComponent<Tower>().m_usedTiles);
                GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_removeBuildingSound);
                UIElementReference.Instance.m_towerUpgradeTowerAttackPowerButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerUpgradeTowerAttackSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerUpgradeTowerAttackRangeButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerTargetConditionFirstTargetButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerTargetConditionLastTargetButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerTargetConditionMaxHealthButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerTargetConditionMinHealthButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerTargetConditionMaxSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIElementReference.Instance.m_towerTargetConditionMinSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();

                UIElementReference.Instance.m_towerUpgradePanel.SetActive(false);
            });
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            UIElementReference.Instance.m_towerUpgradeTowerAttackPowerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerUpgradeTowerAttackSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerUpgradeTowerAttackRangeButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerTargetConditionFirstTargetButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerTargetConditionLastTargetButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerTargetConditionMaxHealthButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerTargetConditionMinHealthButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerTargetConditionMaxSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_towerTargetConditionMinSpeedButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIElementReference.Instance.m_removeTowerButton.GetComponent<Button>().onClick.RemoveAllListeners();

            if (m_selectedTower != null)
                m_selectedTower.GetComponent<Tower>().TurnOffRangeIndiactor();

            m_selectedTower = null;

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
