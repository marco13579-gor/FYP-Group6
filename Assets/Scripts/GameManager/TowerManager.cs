using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TowerManager : Singleton<TowerManager>
{
    public Dictionary<int, GameObject> m_towers = new Dictionary<int, GameObject>();

    private void Start()
    {
        SetUpListeners();
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
            m_towers.Add(randomIndex, gameObj);
            gameObj.GetComponent<Tower>().AssignID(randomIndex);

            return randomIndex;
        }
        else
        {
            Debug.LogError("Invalid Tower: " + gameObj.name);
            return -1;
        }
    }

    public void Unregister(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Tower tower))
        {
            m_towers.Remove(tower.GetTowerID());
        }
        else
        {
            Debug.LogError("Invalid Tower: " + gameObj.name);
        }
    }
}
