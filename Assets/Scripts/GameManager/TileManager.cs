using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : NetworkedSingleton<TileManager>
{
    [SerializeField]
    private Material m_tileMat;
    [SerializeField]
    private Material m_tilePlacedMat;

    private Dictionary<int, GameObject> m_tiles = new Dictionary<int, GameObject>();

    private void Start()
    {
        SetUpListeners();
    }

    private void SetUpListeners()
    {
        GameEventReference.Instance.OnTowerPlaced.AddListener(OnTowerPlaced);
        GameEventReference.Instance.OnTowerRemoved.AddListener(OnTowerRemoved);
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

    private void OnTowerRemoved(params object[] param)
    {
        GameObject[] m_usedTiles = (GameObject[])param[1];

        int[] tilesIDList = new int[m_usedTiles.Length];

        for (int i = 0; i < m_usedTiles.Length; i++)
        {
            tilesIDList[i] = m_usedTiles[i].GetComponent<Tiles>().GetTilesID();
        }

        for (int i = 0; i < tilesIDList.Length; i++)
        {
            GameObject tileToRemovePlaced = m_tiles[tilesIDList[i]];
            tileToRemovePlaced.GetComponent<Tiles>().m_isPlaced = false;
        }
        RemoveTilesPlacedServerRpc(tilesIDList);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveTilesPlacedServerRpc(int[] tilesIDList)
    {
        RemoveTilesPlacedClientRpc(tilesIDList);
    }

    [ClientRpc]
    private void AddTilesPlacedClientRpc(int[] m_tileToBuild, int tileID)
    {
        m_tiles[tileID].GetComponent<Tiles>().m_isPlaced = true;
        m_tiles[tileID].GetComponent<Tiles>().GetComponent<MeshRenderer>().material = m_tilePlacedMat;

        for (int i = 0; i < m_tileToBuild.Length; i++)
        {
            GameObject currentNearbyTile = m_tiles[tileID].GetComponent<Tiles>().m_tilesNearby[m_tileToBuild[i]].gameObject;
            currentNearbyTile.GetComponent<Tiles>().m_isPlaced = true;
            currentNearbyTile.GetComponent<Tiles>().GetComponent<MeshRenderer>().material = m_tilePlacedMat;
        }
    }

    [ClientRpc]
    private void RemoveTilesPlacedClientRpc(int[] m_tileToBuild)
    {
        print("RemoveTilesPlacedClientRpc");
        for (int i = 0; i < m_tileToBuild.Length; i++)
        {
            GameObject tileToRemovePlaced = m_tiles[m_tileToBuild[i]];
            tileToRemovePlaced.GetComponent<Tiles>().m_isPlaced = false;
            tileToRemovePlaced.GetComponent<Tiles>().GetComponent<MeshRenderer>().material = m_tileMat;
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
            Debug.LogError("Invalid Tile: " + gameObj.name);
        }
    }
}
