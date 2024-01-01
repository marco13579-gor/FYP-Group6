using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BuildingManager : NetworkedSingleton<BuildingManager>
{
    [SerializeField] Camera m_mainCamera;
    [SerializeField] PrebuildTower m_prebuildTower = null;
    [SerializeField] LayerMask m_layerMask;

    public float m_gridSize;

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
            Vector3 offset = m_prebuildTower.m_towerSO.m_offset;
            m_prebuildTower.transform.position = raycastHit.transform.gameObject.transform.position - new Vector3(0.5f, -1, 0.5f) + m_prebuildTower.m_towerSO.m_offset;

            //case for tower use extra tile
            if (Input.GetMouseButtonDown(0) && !raycastHit.transform.gameObject.GetComponent<Node>().m_isPlaced && m_prebuildTower.m_towerSO.m_tileToBuild.Length >= 0)
            {
                bool canBuildTower = true;

                for (int i = 0; i < m_prebuildTower.m_towerSO.m_tileToBuild.Length; i++)
                {
                    if (raycastHit.transform.gameObject.GetComponent<Tiles>().m_tilesNearby[m_prebuildTower.m_towerSO.m_tileToBuild[i]] == null || raycastHit.transform.gameObject.GetComponent<Tiles>().m_tilesNearby[m_prebuildTower.m_towerSO.m_tileToBuild[i]].gameObject.GetComponent<Tiles>().m_isPlaced)
                    {
                        canBuildTower = false;
                    }
                }

                if (canBuildTower)
                {
                    //BuildTowerServerRPC(raycastHit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, m_towerToBuild.GetTowerID());
                    //raycastHit.transform.gameObject.GetComponent<Tiles>().m_isPlaced = true;
                    //GameEventReference.Instance.OnTowerPlaced.Trigger(m_towerToBuild.GetTowerID(), m_towerToBuild.m_towerSO, raycastHit.transform.gameObject, m_towerToBuild);
                    //m_towerToBuild = null;
                    ToBuildTowerParam param = new ToBuildTowerParam();
                    param.toBuildTower = m_prebuildTower.GetTower();
                    param.tile = raycastHit.transform.gameObject;

                    //FixedString128Bytes paramToString = JsonUtility.ToJson(param, false);

                    BuildingRequestServerRpc(m_prebuildTower.GetTower().m_towerSO.m_towerType, raycastHit.transform.GetComponent<Tiles>(), m_prebuildTower.transform.position);
                }
            }

            //Delete tower
            if (Input.GetMouseButtonDown(1))
            {
                int newGoldAmount = PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) + m_prebuildTower.GetTower().m_towerSO.m_cost;

                GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGoldAmount, GameNetworkManager.Instance.GetPlayerID());

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
        if(NetworkManager.Singleton.LocalClientId == clientId && (IsServer || IsHost))
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

    [ClientRpc]
    private void DestroyTowerClientRpc(ulong id)
    {
        if(id  == NetworkManager.Singleton.LocalClientId)
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
        if (m_prebuildTower != null)
        {
            Debug.Log("Waiting to build a tower!");
        }
        else if (PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) >= tower.m_towerSO.m_cost)
        {
            int newGoldAmount = PlayerStatsManager.Instance.GetPlayerGold(GameNetworkManager.Instance.GetPlayerID()) - tower.m_towerSO.m_cost;

            print($"newGoldAmount: {newGoldAmount}");
            print($"GameNetworkManager.Instance.GetPlayerID(): {GameNetworkManager.Instance.GetPlayerID()}");

            GameEventReference.Instance.OnPlayerModifyGold.Trigger(newGoldAmount, GameNetworkManager.Instance.GetPlayerID());
            m_prebuildTower = Instantiate(tower);
            //m_towerToBuild.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, .5f);
        }
        else
        {
            Debug.Log("Cost is too high!!");
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
}

[Serializable]
public struct ToBuildTowerParam
{
    public GameObject tile;
    public Tower toBuildTower;
}