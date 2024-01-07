using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : NetworkedSingleton<TileManager>
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
        int[] m_tileToBuild = towerSO.m_tileToBuild;

        AddTilesPlacedClientRpc(m_tileToBuild, tileID);
    }

    [ClientRpc]
    private void AddTilesPlacedClientRpc(int[] m_tileToBuild, int tileID)
    {
        print("AddTilesPlacedClientRpc");
        m_tiles[tileID].GetComponent<Tiles>().m_isPlaced = true;

        for (int i = 0; i < m_tileToBuild.Length; i++)
        {
            GameObject currentNearbyTile = m_tiles[tileID].GetComponent<Tiles>().m_tilesNearby[m_tileToBuild[i]].gameObject;
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

            TileRigisterClientRpc(randomIndex, gameObj.GetComponent<Tiles>());
            //m_tiles.Add(randomIndex, gameObj);

            return randomIndex;
        }
        else
        {
            Debug.LogError("Invalid Tiles: " + gameObj.name);
            return -1;
        }
    }

    [ClientRpc]
    private void TileRigisterClientRpc(int tileID, NetworkBehaviourReference tileParam)
    {
        Tiles tile;
        tileParam.TryGet(out tile);

        GameObject tileGameObject = tile.gameObject;

        m_tiles.Add(tileID, tileGameObject);
        tile.SetTilesID(tileID);
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
