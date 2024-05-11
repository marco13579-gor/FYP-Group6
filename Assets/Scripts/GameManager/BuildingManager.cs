using System;
using Unity.Netcode;
using UnityEngine;

public class BuildingManager : NetworkedSingleton<BuildingManager>
{
    [SerializeField] Camera m_mainCamera;
    [SerializeField] PrebuildTower m_prebuildTower = null;
    [SerializeField] LayerMask m_layerMask;

    private int m_cardSlotToConsume;

    public float m_gridSize;

    private void Start()
    {
        GameEventReference.Instance.OnTowerRemoved.AddListener(OnTowerRemoved);
    }

    public void Update()
    {
        if (m_prebuildTower != null)
        {
            PrepareToBuild();
        }
    }

    public void PrepareToBuild()
    {
        Ray ray = m_mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, m_layerMask))
        {
            GameObject currentTile = raycastHit.transform.gameObject;

            Vector3 offset = m_prebuildTower.m_towerSO.m_offset;
            m_prebuildTower.transform.position = raycastHit.transform.gameObject.transform.position - new Vector3(0.25f, m_prebuildTower.m_towerSO.m_offset.y - currentTile.transform.localScale.y * 1.5f / 2f, 0.25f) + m_prebuildTower.m_towerSO.m_offset;

            if (!currentTile.GetComponent<Node>().m_isPlaced
                && currentTile.GetComponent<Node>().GetPossibleBuilderID() == GameNetworkManager.Instance.GetPlayerID()
                && m_prebuildTower.m_towerSO.m_tileToBuild.Length >= 0)
            {
                bool canBuildTower = true;

                //checking extra tiles
                for (int i = 0; i < m_prebuildTower.m_towerSO.m_tileToBuild.Length; i++)
                {
                    if (currentTile.GetComponent<Tiles>().m_tilesNearby[m_prebuildTower.m_towerSO.m_tileToBuild[i]] == null
                        || currentTile.GetComponent<Tiles>().m_tilesNearby[m_prebuildTower.m_towerSO.m_tileToBuild[i]].gameObject.GetComponent<Tiles>().m_isPlaced)
                    {
                        canBuildTower = false;
                    }
                }

                if (canBuildTower)
                {
                    foreach (MeshRenderer meshRenderer in m_prebuildTower.gameObject.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRenderer.material.color = new Color(0.2216f, 0.4774f, 1f, 0.7215f);
                    }
                }
                else
                {
                    foreach (MeshRenderer meshRenderer in m_prebuildTower.gameObject.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRenderer.material.color = new Color(0.9811f, 0.2267f, 0.2267f, 0.7215f);
                    }
                }

                if (Input.GetMouseButtonDown(0) && canBuildTower)
                {
                    ToBuildTowerParam param = new ToBuildTowerParam();
                    param.toBuildTower = m_prebuildTower.GetTower();
                    param.tile = raycastHit.transform.gameObject;

                    GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_buildSound);
                    GameEventReference.Instance.OnPlayerConsumeCard.Trigger(m_cardSlotToConsume);
                    BuildingRequestServerRpc(m_prebuildTower.GetTower().m_towerSO.m_towerType, raycastHit.transform.GetComponent<Tiles>(), m_prebuildTower.transform.position);
                    WarningManager.Instance.ModifyCardSlotWarningText("");
                }
                else if(Input.GetMouseButtonDown(0) && !canBuildTower)
                {
                    WarningManager.Instance.ModifyCardSlotWarningText("Tiles are occupy by other towers. Please select another tiles to build.");
                    GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_wrongSound);
                }
            }
            else
            {
                foreach (MeshRenderer meshRenderer in m_prebuildTower.gameObject.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material.color = new Color(0.9811f, 0.2267f, 0.2267f, 0.7215f);
                }
            }

            //Delete tower
            if (Input.GetMouseButtonDown(1))
            {
                int newGoldAmount = PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) + m_prebuildTower.GetTower().m_towerSO.m_cost;

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGoldAmount, GameNetworkManager.Instance.GetPlayerID());

                WarningManager.Instance.ModifyCardSlotWarningText($"");

                Destroy(m_prebuildTower.gameObject);
                m_prebuildTower = null;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void BuildTowerServerRpc(NetworkBehaviourReference toBuildTowerParam, NetworkBehaviourReference tileParam, Vector3 position, ServerRpcParams serverRpcParam)
    {
        Tower tower;
        Tiles tiles;
        toBuildTowerParam.TryGet(out tower);
        tileParam.TryGet(out tiles);

        ulong clientId = serverRpcParam.Receive.SenderClientId;
        if (NetworkManager.Singleton.LocalClientId == clientId && (IsServer || IsHost))
        {
            Destroy(m_prebuildTower.gameObject);
            m_prebuildTower = null;
        }
        else
        {
            DestroyTowerClientRpc(serverRpcParam.Receive.SenderClientId);
        }

        tower.SetTowerID(TowerManager.Instance.Register(tower.gameObject));
        GameEventReference.Instance.OnTowerPlaced.Trigger(tower.GetTowerID(), tower.m_towerSO, tiles.gameObject, tower);
    }

    private void OnTowerRemoved(params object[] param)
    {
        int towerID = (int)param[0];
        RemoveTowerServerRpc(towerID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTowerServerRpc(int towerID)
    {
        if (TowerManager.Instance.m_towers.ContainsKey(towerID))
        {
            GameObject towerToRemove = TowerManager.Instance.m_towers[towerID];

            TowerManager.Instance.Unregister(TowerManager.Instance.m_towers[towerID]);
            towerToRemove.GetComponent<NetworkObject>().Despawn();
        }
    }

    [ClientRpc]
    private void DestroyTowerClientRpc(ulong id)
    {
        if (id == NetworkManager.Singleton.LocalClientId)
        {
            Destroy(m_prebuildTower.gameObject);
            m_prebuildTower = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void BuildingRequestServerRpc(TowerType towerType, NetworkBehaviourReference tileParam, Vector3 position, ServerRpcParams serverRpcParam = default)
    {
        Tower tower;
        switch (towerType)
        {
            case TowerType.Tower1x1:
                tower = TowerPrefabsReference.Instance.m_tower1x1.GetComponent<Tower>();
                break;
            case TowerType.Tower2x2:
                tower = TowerPrefabsReference.Instance.m_tower2x2.GetComponent<Tower>();
                break;
            case TowerType.Tower1x2:
                tower = TowerPrefabsReference.Instance.m_tower1x2.GetComponent<Tower>();
                break;
            case TowerType.HotCoalsTower:
                tower = TowerPrefabsReference.Instance.m_hotCoalTower.GetComponent<Tower>();
                break;
            case TowerType.Inferno:
                tower = TowerPrefabsReference.Instance.m_inferno.GetComponent<Tower>();
                break;
            case TowerType.TheDevilDeal:
                tower = TowerPrefabsReference.Instance.m_theDevilTower.GetComponent<Tower>();
                break;
            case TowerType.EvilTower:
                tower = TowerPrefabsReference.Instance.m_evilTower.GetComponent<Tower>();
                break;
            case TowerType.Lucifer:
                tower = TowerPrefabsReference.Instance.m_lucifer.GetComponent<Tower>();
                break;
            case TowerType.SatanTower:
                tower = TowerPrefabsReference.Instance.m_satanTower.GetComponent<Tower>();
                break;
            case TowerType.BlazeTower:
                tower = TowerPrefabsReference.Instance.m_blazeTower.GetComponent<Tower>();
                break;
            case TowerType.CrystalTower:
                tower = TowerPrefabsReference.Instance.m_crystalTower.GetComponent<Tower>();
                break;
            case TowerType.FreezeTower:
                tower = TowerPrefabsReference.Instance.m_freezeTower.GetComponent<Tower>();
                break;
            case TowerType.IceTower:
                tower = TowerPrefabsReference.Instance.m_iceTower.GetComponent<Tower>();
                break;
            case TowerType.TempestTower:
                tower = TowerPrefabsReference.Instance.m_tempestTower.GetComponent<Tower>();
                break;
            case TowerType.Pertubable:
                tower = TowerPrefabsReference.Instance.m_pertubable.GetComponent<Tower>();
                break;
            case TowerType.Disaster:
                tower = TowerPrefabsReference.Instance.m_disasterTower.GetComponent<Tower>();
                break;
            case TowerType.HardSteelTower:
                tower = TowerPrefabsReference.Instance.m_hardSteel.GetComponent<Tower>();
                break;
            case TowerType.UnstableTower:
                tower = TowerPrefabsReference.Instance.m_unstableTower.GetComponent<Tower>();
                break;
            case TowerType.BountryTower:
                tower = TowerPrefabsReference.Instance.m_bountryTower.GetComponent<Tower>();
                break;
            case TowerType.GoldMine:
                tower = TowerPrefabsReference.Instance.m_goldMiner.GetComponent<Tower>();
                break;
            case TowerType.RedCrimson:
                tower = TowerPrefabsReference.Instance.m_redCrimson.GetComponent<Tower>();
                break;
            case TowerType.AncientRelics:
                tower = TowerPrefabsReference.Instance.m_ancientRlics.GetComponent<Tower>();
                break;
            case TowerType.Obelisk:
                tower = TowerPrefabsReference.Instance.m_obelisk.GetComponent<Tower>();
                break;
            case TowerType.Chronos:
                tower = TowerPrefabsReference.Instance.m_chronos.GetComponent<Tower>();
                break;
            case TowerType.TheDespair:
                tower = TowerPrefabsReference.Instance.m_theDespair.GetComponent<Tower>();
                break;
            case TowerType.AbsoluteZero:
                tower = TowerPrefabsReference.Instance.m_absoluteZero.GetComponent<Tower>();
                break;
            default:
                tower = null;
                break;
        }
        Tower towerToBuild = Instantiate(tower);
        towerToBuild.m_isPlaced = true;
        towerToBuild.transform.position = position;
        towerToBuild.GetComponent<NetworkObject>().Spawn();

        BuildTowerServerRpc(towerToBuild, tileParam, position, serverRpcParam);
    }

    public void SelectTower(PrebuildTower tower)
    {
        if (PlayerStatsManager.Instance.GetLoseStatus())
        {
            WarningManager.Instance.ModifyCardSlotWarningText("You cannot build when you lose the game!");
            return;
        }

        if (m_prebuildTower != null)
        {
            WarningManager.Instance.ModifyCardSlotWarningText("Please place the tower before selecting other tower, right click to deselect.");
        }
        else if (PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) >= tower.m_towerSO.m_cost)
        {
            int newGoldAmount = PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - tower.m_towerSO.m_cost;

            GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGoldAmount, GameNetworkManager.Instance.GetPlayerID());
            m_prebuildTower = Instantiate(tower);
            WarningManager.Instance.ModifyCardSlotWarningText("You are entered building mode. Left click to build or right click to deselect.");
            //m_towerToBuild.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, .5f);
        }
        else
        {
            Debug.Log("Cost is too high!!");
            GameObjectReference.Instance.m_audioSource.PlayOneShot(AudioClipReference.Instance.m_wrongSound);
            WarningManager.Instance.ModifyCardSlotWarningText($"The tower require ${tower.m_towerSO.m_cost} and you only own ${PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID())}");
        }
    }
    public float RoundToNearestGrid(float pos)
    {
        float xDiff = pos % m_gridSize;
        pos -= xDiff;
        if (xDiff > m_gridSize / 2)
        {
            pos += m_gridSize;
        }
        return pos;
    }


    public Vector3 RoundToNearestGrid(GameObject tile, Vector3 pos)
    {
        pos.x = Mathf.Floor(pos.x * 10) / 10;
        pos.y = Mathf.Floor(pos.y * 10) / 10;
        pos.z = Mathf.Floor(pos.z * 10) / 10;

        float xDiff = pos.x == 0 ? 0 : pos.x % m_gridSize;
        float yDiff = pos.y == 0 ? 0 : pos.y % m_gridSize;
        float zDiff = pos.z == 0 ? 0 : pos.z % m_gridSize;

        pos -= new Vector3(xDiff, yDiff, zDiff);

        pos.x += xDiff > m_gridSize / 2 ? m_gridSize : 0;
        pos.y += yDiff > m_gridSize / 2 ? m_gridSize : 0;
        pos.z += zDiff > m_gridSize / 2 ? m_gridSize : 0;
        return pos;
    }

    public void SetCardConsumeSlot(int cardSlotIndex) => m_cardSlotToConsume = cardSlotIndex;
    public PrebuildTower GetPrebuildTower() => m_prebuildTower;
    public void RemovePrebuildTower() => m_prebuildTower = null;
}

[Serializable]
public struct ToBuildTowerParam
{
    public GameObject tile;
    public Tower toBuildTower;
}