using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : Singleton<TileManager>
{
    private Dictionary<int, GameObject> m_tiles = new Dictionary<int, GameObject>();

    private void Start()
    {
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnTowerPlaced.AddListener(OnTowerPlaced);
    }

    private void OnTowerPlaced(params object[] param)
    {
        int towerID = (int)param[0];
        TowerSO towerSO = (TowerSO)param[1];
        GameObject originTile = (GameObject)param[2];

        int tileID = originTile.GetComponent<Tiles>().GetTilesID();

        m_tiles[tileID].GetComponent<Tiles>().m_isPlaced = true;

        for (int i = 0; i < towerSO.m_tileToBuild.Length; i++)
        {
            GameObject currentNearbyTile = m_tiles[tileID].GetComponent<Tiles>().m_tilesNearby[towerSO.m_tileToBuild[i]].gameObject;
            currentNearbyTile.GetComponent<Tiles>().m_isPlaced = true;
        }
    }

    public int Register(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Tiles tiles))
        {
            //Detect if the list was full
            if (m_tiles.Count >= 16777217)
            {
                Debug.LogError("Failed To Register Tiles: " + gameObj.name + "\nTiles List Is Full!");
            }

            //arrange valid index
            int randomIndex = Random.Range(0, 16777216);
            while (m_tiles.ContainsKey(randomIndex))
            {
                randomIndex = Random.Range(0, 16777216);
            }

            //Add to list
            m_tiles.Add(randomIndex, gameObj);

            return randomIndex;
        }
        else
        {
            Debug.LogError("Invalid Tiles: " + gameObj.name);
            return -1;
        }
    }

    public void Unregister(GameObject gameObj)
    {
        if (gameObj.TryGetComponent(out Tiles tile))
        {
            m_tiles.Remove(tile.GetTilesID());
        }
        else
        {
            Debug.LogError("Invalid Tower: " + gameObj.name);
        }
    }
}
